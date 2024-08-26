namespace EventSourcingExample.Api.Events.Toys;

public record UpdateToyPrice(Guid Id, decimal Price) : ToyEvent;