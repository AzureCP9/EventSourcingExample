using EventSourcingExample.Api.Controllers.Commands;
using EventSourcingExample.Api.Domain.Toys;
using EventSourcingExample.Api.Services;
using MediatR;

namespace EventSourcingExample.Api.Controllers.Handlers;

public class GetToysProjectionCommandHandler : IRequestHandler<GetToysProjectionCommand, List<Toy>>
{
    private readonly ToyService _toyService;

    public GetToysProjectionCommandHandler(ToyService toyService) => _toyService = toyService;

    public async Task<List<Toy>> Handle(GetToysProjectionCommand request, CancellationToken cancellationToken) =>
        await _toyService.GetToysAsync();
}
