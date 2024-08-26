namespace EventSourcingExample.Api.Db.Entities;

public class ProjectionCheckpointDbEntity
{
    public long Id { get; private set; }
    public required string ProjectionName { get; set; }
    public required long StreamCheckpoint { get; set; }
}
