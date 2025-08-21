using UmlViewer.UI.ModelView;
using Validation;

namespace UmlViewer.UI;

/// <summary>
/// Builds an FsmView from the Validation.FsmRepository indexes.
/// </summary>
public sealed class RepositoryFsmViewBuilder : IFsmViewBuilder
{
    private readonly FsmRepository _repo;

    public RepositoryFsmViewBuilder(FsmRepository repo)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
    }

    public FsmView Build(State root, string title = "FSM (validated)")
        => throw new NotSupportedException("This builder works from FsmRepository.RootState. Use BuildFromRepository().");

    public FsmView BuildFromRepository(string title = "FSM")
    {
        if (_repo.RootState is null)
            throw new InvalidOperationException("Repository has no RootState (initial).");

        var allStates = CollectStates(_repo.RootState);

        var viewById = new Dictionary<string, StateView>(StringComparer.Ordinal);
        foreach (var rs in allStates)
        {
            viewById[rs.Id] = new StateView
            {
                Identifier = rs.Id,
                DisplayName = rs.Name,             // friendly name from RawState
                IsCompound = rs.type == StateType.COMPOUND
            };
        }

        foreach (var rs in allStates)
        {
            if (!_repo.Actions.TryGetValue(rs.Id, out var actions)) continue;

            var sv = viewById[rs.Id];
            foreach (var a in actions)
            {
                switch (a.Type)
                {
                    case ActionType.ENTRY_ACTION: sv.EntryActions.Add(a.Description); break;
                    case ActionType.DO_ACTION:    sv.DoActions.Add(a.Description);    break;
                    case ActionType.EXIT_ACTION:  sv.ExitActions.Add(a.Description);  break;
                    // TRANSITION_ACTION belongs to transitions (handled later)
                }
            }
        }

        var roots = new List<StateView>();
        foreach (var rs in allStates)
        {
            if (rs.ParentId is null)
            {
                roots.Add(viewById[rs.Id]);
                continue;
            }

            if (viewById.TryGetValue(rs.ParentId, out var parentView))
                parentView.Children.Add(viewById[rs.Id]);
        }

        var topLevelTransitions = new List<TransitionView>();
        foreach (var rs in allStates)
        {
            if (!_repo.SourceTransitions.TryGetValue(rs.Id, out var outgoing)) continue;

            var fromView = viewById[rs.Id];

            foreach (var t in outgoing)
            {
                var toDisplay = viewById.TryGetValue(t.DestinationStateId, out var toV)
                    ? toV.DisplayName
                    : t.DestinationStateId;

                var tv = new TransitionView
                {
                    FromIdentifier = t.SourceStateId,
                    ToIdentifier = t.DestinationStateId,
                    ToDisplayName = toDisplay,
                    TriggerIdentifier = t.TriggerId,          
                    GuardCondition = t.GuardCondition
                };

                if (_repo.Actions.TryGetValue(t.Id, out var tActions))
                {
                    foreach (var a in tActions.Where(x => x.Type == ActionType.TRANSITION_ACTION))
                        tv.TransitionActions.Add(a.Description);
                }

                if (viewById.ContainsKey(t.SourceStateId))
                    fromView.Outgoing.Add(tv);
                else
                    topLevelTransitions.Add(tv);
            }
        }

        // 6) Initial / Final
        StateView? initial = null, final = null;
        foreach (var rs in allStates)
        {
            if (rs.type == StateType.INITIAL) initial = viewById[rs.Id];
            if (rs.type == StateType.FINAL)   final   = viewById[rs.Id];
        }

        // 7) Assemble
        var view = new FsmView
        {
            Title = title,
            Initial = initial,
            Final = final
        };
        view.RootStates.AddRange(roots);
        view.TopLevelTransitions.AddRange(topLevelTransitions);
        return view;
    }

    private List<RawState> CollectStates(RawState root)
    {
        var result = new List<RawState>();
        var stack = new Stack<RawState>();
        var seen  = new HashSet<string>(StringComparer.Ordinal);

        stack.Push(root);
        while (stack.Count > 0)
        {
            var s = stack.Pop();
            if (!seen.Add(s.Id)) continue;

            result.Add(s);

            if (_repo.RawChildren.TryGetValue(s.Id, out var children))
            {
                for (int i = children.Count - 1; i >= 0; i--)
                    stack.Push(children[i]);
            }
        }
        return result;
    }
}
