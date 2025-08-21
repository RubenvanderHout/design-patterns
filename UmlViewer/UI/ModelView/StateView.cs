namespace UmlViewer.UI.ModelView;

public sealed class StateView
{
    public required string Identifier { get; init; }
    public required string DisplayName { get; init; }
    public bool IsCompound { get; init; }

    public List<string> EntryActions { get; } = new();
    public List<string> DoActions { get; } = new();
    public List<string> ExitActions { get; } = new();

    public List<StateView> Children { get; } = new();
    public List<TransitionView> Outgoing { get; } = new();
}