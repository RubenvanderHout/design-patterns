namespace IO.DTO;
public sealed class StateDto
{
    public required string Identifier { get; init; }
    public string? Parent { get; init; } // "_" => null
    public required string Name { get; init; }
    public required StateType Type { get; init; }
}