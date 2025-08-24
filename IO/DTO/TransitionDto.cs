namespace IO.DTO;
public sealed record  class TransitionDto : DiagramComponentDto
{
    public required string SourceStateIdentifier { get; init; }
    public required string DestinationStateIdentifier { get; init; }
    public string? TriggerIdentifier { get; init; } 
    public required string GuardCondition { get; init; } 
}