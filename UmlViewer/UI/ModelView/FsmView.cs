namespace UmlViewer.UI.ModelView;

public sealed class FsmView
{
    public string Title { get; }
    public StateView Initial { get; }
    public StateView Final { get; }

    private readonly List<StateView> _rootStates;
    private readonly List<TransitionView> _topLevelTransitions;

    public IReadOnlyList<StateView> RootStates => _rootStates;
    public IReadOnlyList<TransitionView> TopLevelTransitions => _topLevelTransitions;

    private FsmView(
        string title,
        StateView initial,
        StateView final,
        IEnumerable<StateView> rootStates,
        IEnumerable<TransitionView> topLevelTransitions)
    {
        Title = title;
        Initial = initial;
        Final = final;
        _rootStates = rootStates?.ToList() ?? new();
        _topLevelTransitions = topLevelTransitions?.ToList() ?? new();
    }

    public static FsmView Create(
        string title,
        StateView initial,
        StateView final,
        IEnumerable<StateView> rootStates,
        IEnumerable<TransitionView> topLevelTransitions)
        => new(title, initial, final, rootStates, topLevelTransitions);
}