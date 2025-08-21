namespace UmlViewer.UI.ModelView;

public sealed class FsmView
{
    public string Title { get; init; } = "FSM";
    public required StateView Initial { get; init; }
    public required StateView Final { get; init; }
    public List<StateView> RootStates { get; } = new();
    public List<TransitionView> TopLevelTransitions { get; } = new();
}