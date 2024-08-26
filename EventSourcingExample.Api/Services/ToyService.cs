using EventSourcingExample.Api.Db;
using EventSourcingExample.Api.Domain.Toys;
using EventSourcingExample.Api.Events.Toys;
using EventStore.Client;
using System.Runtime.Serialization;
using System.Text.Json;

namespace EventSourcingExample.Api.Services;

public class ToyService
{
    private readonly ILogger<ToyService> _logger;
    private readonly EventStoreClient _eventStoreClient;
    private readonly ToyDbContext _toyDbContext;

    public ToyService(EventStoreClient eventStoreClient, ILogger<ToyService> logger, ToyDbContext toyDbContext) =>
        (_eventStoreClient, _logger, _toyDbContext) = (eventStoreClient, logger, toyDbContext);

    public async Task CreateToyAsync(CreateToy @event)
    {
        var eventData = new EventData(
            Uuid.NewUuid(),
            nameof(CreateToy),
            JsonSerializer.SerializeToUtf8Bytes(@event));

        await _eventStoreClient.AppendToStreamAsync(
            $"toy-{@event.Id}",
            StreamState.Any,
            [eventData]);
    }

    public async Task UpdateToyPriceAsync(UpdateToyPrice @event)
    {
        var eventData = new EventData(
           Uuid.NewUuid(),
           nameof(UpdateToyPrice),
           JsonSerializer.SerializeToUtf8Bytes(@event));

        await _eventStoreClient.AppendToStreamAsync(
            $"toy-{@event.Id}",
            StreamState.Any,
            [eventData]);
    }
    public async Task<List<Toy>> GetToysAsync()
    {
        var toys = new Dictionary<Guid, Toy>();
        var events = _eventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            streamName: "$ce-toy",
            StreamPosition.Start,
            resolveLinkTos: true
        );

        await foreach (var @event in events)
        {
            var eventType = @event.Event.EventType;
            var eventData = @event.Event.Data;

            ToyEvent? toyEvent = eventType switch
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
                },
                _ => throw new InvalidOperationException($"No such event: {eventType}")
            };

            handleEvent.Invoke();

            _logger.LogInformation("Event: {event}", @event);
        }

        return toys.Values.ToList();
    }
}
