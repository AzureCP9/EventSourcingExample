namespace EventSourcingExample.Api.Events.Toys;

public record CreateToy(Guid Id, string Name, decimal Price) : ToyEvent; 