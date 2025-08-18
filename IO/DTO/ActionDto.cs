namespace IO.DTO;

public sealed class ActionDto
{
    public required string Identifier { get; init; }
    public required string Description { get; init; }
    public required ActionType Type { get; init; }
}
