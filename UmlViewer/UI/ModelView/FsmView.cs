namespace UmlViewer.UI.ModelView;

public sealed class FsmView
{
    public string Title { get; init; } = "FSM";
    public StateView? Initial { get; init; }
    public StateView? Final { get; init; }
    public List<StateView> RootStates { get; } = new();
    public List<TransitionView> TopLevelTransitions { get; } = new();

}