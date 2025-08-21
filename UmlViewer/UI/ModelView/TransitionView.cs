namespace UmlViewer.UI.ModelView;

public sealed class TransitionView
{
    public required string FromIdentifier { get; init; }
    public required string ToIdentifier { get; init; }
    public required string ToDisplayName { get; init; }
    public string? TriggerIdentifier { get; init; }
    public string GuardCondition { get; init; } = "";
    public List<string> TransitionActions { get; } = new();
}