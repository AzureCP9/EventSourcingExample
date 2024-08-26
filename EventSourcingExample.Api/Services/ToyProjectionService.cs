using EventSourcingExample.Api.Db;
using EventSourcingExample.Api.Db.Entities;
using EventSourcingExample.Api.Domain.Toys;
using EventSourcingExample.Api.Events.Toys;
using EventSourcingExample.Api.Resilience;
using EventStore.Client;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;
using System.Text.Json;

namespace EventSourcingExample.Api.Services;

public class ToyProjectionService : BackgroundService
{
    private readonly EventStoreClient _eventStoreClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ToyProjectionService> _logger;
    private readonly ProjectionPolicy _projectionPolicy;

    public ToyProjectionService(
        EventStoreClient eventStoreClient,
        IServiceScopeFactory scopeFactory,
        ILogger<ToyProjectionService> logger,
        ProjectionPolicy projectionPolicy)
    {
        _eventStoreClient = eventStoreClient;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _projectionPolicy = projectionPolicy;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken) =>
        await _projectionPolicy.ExecuteAsync(ProcessEventsAsync, cancellationToken);

    private async Task ProcessEventsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Processing events");
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ToyDbContext>();

            var lastProcessedPosition = dbContext.ProjectionCheckpoint.FirstOrDefault(pc => pc.ProjectionName == nameof(ToyProjectionService))?.StreamCheckpoint
                ?? StreamPosition.Start.ToInt64();

            var events = _eventStoreClient.ReadStreamAsync(
                Direction.Forwards,
                streamName: "$ce-toy",
                StreamPosition.FromInt64(lastProcessedPosition),
                resolveLinkTos: true,
                cancellationToken: cancellationToken
            );

            var toys = new Dictionary<Guid, Toy>();

            await foreach (var @event in events)
            {
                var eventType = @event.Event.EventType;
                var eventData = @event.Event.Data;

                ToyEvent toyEvent = eventType switch
                {
                    nameof(CreateToy) => JsonSerializer.Deserialize<CreateToy>(eventData.Span) ?? throw new SerializationException("Did not deserialize properly"),
                    nameof(UpdateToyPrice) => JsonSerializer.Deserialize<UpdateToyPrice>(eventData.Span) ?? throw new SerializationException("Did not deserialize properly"),
                    _ => throw new InvalidOperationException($"Wtf kinda event is {eventType}")
                };

                Action handleEvent = toyEvent switch
                {
                    CreateToy createToy => () =>
                    {
                        if (!toys.ContainsKey(createToy.Id))
                            toys[createToy.Id] = Toy.Apply(createToy);
                    }
                    ,
                    UpdateToyPrice updateToyPrice => () =>
                    {
                        if (toys.ContainsKey(updateToyPrice.Id))
                            toys[updateToyPrice.Id] = toys[updateToyPrice.Id].Apply(updateToyPrice);
                    }
                    ,
                    _ => throw new InvalidOperationException($"No such event: {eventType}")
                };

                handleEvent.Invoke();
            }

            using var transaction = dbContext.Database.BeginTransaction();
            var toyDbEntities = toys.Values.Select(t => new ToyDbEntity()
            {
                Id = t.Id,
                Name = t.Name,
                Price = t.Price,
            });

            try
            {
                foreach (var toy in toyDbEntities)
                {
                    var toyDb = dbContext.Toy.FirstOrDefault(t => t.Id == toy.Id);

                    if (toyDb != null)
                    {
                        toyDb.Id = toy.Id;
                        toyDb.Name = toy.Name;
                        toyDb.Price = toy.Price;
                    }
                    else
                    {
                        dbContext.Add(toy);
                    }
                }

                await dbContext.SaveChangesAsync();
                await UpdateCheckpointAsync(events, lastProcessedPosition, dbContext, cancellationToken);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create or update projection");
                await transaction.RollbackAsync();
            }
            Thread.Sleep(1000 * 5);
        }
    }

    private async Task UpdateCheckpointAsync(EventStoreClient.ReadStreamResult events, long lastProcessedPosition, ToyDbContext dbContext, CancellationToken cancellationToken)
    {
        var lastEvent = await events.LastOrDefaultAsync();

        if (lastEvent.Equals(default(ResolvedEvent))) return;

        var lastEventPosition = lastEvent.OriginalEventNumber.ToInt64();

        var existingCheckpoint = await dbContext.ProjectionCheckpoint
            .FirstOrDefaultAsync(pc => pc.ProjectionName == nameof(ToyProjectionService), cancellationToken);

        var newCheckpoint = new ProjectionCheckpointDbEntity()
        {
            ProjectionName = nameof(ToyProjectionService),
            StreamCheckpoint = lastEventPosition,
        };

        if (existingCheckpoint == null)
            dbContext.Add(newCheckpoint);
        else
            existingCheckpoint.StreamCheckpoint = lastEventPosition;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
