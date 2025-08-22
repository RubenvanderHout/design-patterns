namespace IO.DTO;

public sealed class ActionDto : DiagramComponentDto
{    public required string Description { get; init; }
    public required ActionType Type { get; init; }
}
