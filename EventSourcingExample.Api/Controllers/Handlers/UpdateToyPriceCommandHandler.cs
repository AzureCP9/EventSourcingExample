using EventSourcingExample.Api.Controllers.Commands;
using EventSourcingExample.Api.Events.Toys;
using EventSourcingExample.Api.Services;
using MediatR;

namespace EventSourcingExample.Api.Controllers.Handlers;

public class UpdateToyPriceCommandHandler : IRequestHandler<UpdateToyPriceCommand>
{
    private readonly ToyService _toyService;

    public UpdateToyPriceCommandHandler(ToyService toyService) => _toyService = toyService;

    public async Task Handle(UpdateToyPriceCommand request, CancellationToken cancellationToken) =>
        await _toyService.UpdateToyPriceAsync(new UpdateToyPrice(request.id, request.price));
}
