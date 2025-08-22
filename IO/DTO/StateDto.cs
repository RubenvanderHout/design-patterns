namespace IO.DTO;
public sealed class StateDto : DiagramComponentDto
{
    public required string? Parent { get; init; }
    public required string Name { get; init; }
    public required StateType Type { get; init; }
}