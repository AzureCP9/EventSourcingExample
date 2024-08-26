using EventSourcingExample.Api.Controllers.Commands;
using EventSourcingExample.Api.Domain.Toys;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EventSourcingExample.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ToyController : ControllerBase
{
    private readonly ILogger<ToyController> _logger;
    private readonly IMediator _mediator;

    public ToyController(ILogger<ToyController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task CreateToyAsync(string name, decimal price)
    {
        await _mediator.Send(new CreateToyCommand(name, price));
        Created();
    }

    [HttpGet]
    public async Task<List<Toy>> GetToysProjection() =>
        await _mediator.Send(new GetToysProjectionCommand());

    [HttpPost("price/{id:guid}")]
    public async Task<IActionResult> UpdateToyPrice(Guid id, decimal newPrice)
    {
        await _mediator.Send(new UpdateToyPriceCommand(id, newPrice));
        return Ok();
    }

}
