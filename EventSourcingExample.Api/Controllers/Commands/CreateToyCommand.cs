using MediatR;

namespace EventSourcingExample.Api.Controllers.Commands;

public record CreateToyCommand(string name, decimal price) : IRequest;