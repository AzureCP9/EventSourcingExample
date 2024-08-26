using System.ComponentModel.DataAnnotations;

namespace EventSourcingExample.Api.Db.Entities;

public class ToyDbEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
}