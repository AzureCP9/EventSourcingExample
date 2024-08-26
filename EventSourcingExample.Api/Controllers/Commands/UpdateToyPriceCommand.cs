using MediatR;

namespace EventSourcingExample.Api.Controllers.Commands;

public record UpdateToyPriceCommand(Guid id, decimal price) : IRequest;