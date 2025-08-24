namespace IO.DTO;

public sealed record class TriggerDto : DiagramComponentDto
{
    public required string Description { get; init; }
}