using EventSourcingExample.Api.Domain.Toys;
using EventSourcingExample.Api.Events.Toys;
using MediatR;

namespace EventSourcingExample.Api.Controllers.Commands;

public record GetToysProjectionCommand() : IRequest<List<Toy>>;