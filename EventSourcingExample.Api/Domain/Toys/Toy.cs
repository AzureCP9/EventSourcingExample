using EventSourcingExample.Api.Events.Toys;

namespace EventSourcingExample.Api.Domain.Toys;

public record Toy
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public decimal Price { get; init; }

    private Toy(Guid id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }

    public static Toy Create(Guid id, string name, decimal price)
    {
        return new Toy(id, name, price);
    }
    public static Toy Apply(CreateToy @event) =>
        Create(@event.Id, @event.Name, @event.Price);
}

public static class ToyExtensions
{
    public static Toy Apply(this Toy self, UpdateToyPrice @event) =>
       self with { Price = @event.Price };
}