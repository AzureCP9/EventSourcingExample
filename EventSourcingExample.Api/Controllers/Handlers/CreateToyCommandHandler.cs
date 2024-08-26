using EventSourcingExample.Api.Controllers.Commands;
using EventSourcingExample.Api.Events.Toys;
using EventSourcingExample.Api.Services;
using MediatR;
using RT.Comb;

namespace EventSourcingExample.Api.Controllers.Handlers;

public class CreateToyCommandHandler : IRequestHandler<CreateToyCommand>
{
    private readonly ToyService _toyService;

    public CreateToyCommandHandler(ToyService toyService) => _toyService = toyService;

    public async Task Handle(CreateToyCommand request, CancellationToken cancellationToken) =>
        await _toyService.CreateToyAsync(new CreateToy(Provider.Sql.Create(), request.name, request.price));
  
}
