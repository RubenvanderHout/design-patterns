namespace IO.DTO;
public sealed class TransitionDto
{
    public required string Identifier { get; init; }
    public required string SourceStateIdentifier { get; init; }
    public required string DestinationStateIdentifier { get; init; }
    public string? TriggerIdentifier { get; init; } // optioneel
    public required string GuardCondition { get; init; } // lege string indien geen guard
}