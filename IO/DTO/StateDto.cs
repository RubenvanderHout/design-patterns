namespace IO.DTO;
public sealed class StateDto
{
    public required string Identifier { get; init; }
    public required string? Parent { get; init; }
    public required string Name { get; init; }
    public required StateType Type { get; init; }
}