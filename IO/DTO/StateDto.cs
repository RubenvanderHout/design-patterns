namespace IO.DTO;
public sealed record class StateDto : DiagramComponentDto
{
    public required string? Parent { get; init; }
    public required string Name { get; init; }
    public required StateType Type { get; init; }
}