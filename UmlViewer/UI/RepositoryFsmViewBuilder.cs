using UmlViewer.UI.ModelView;
using Validation;

namespace UmlViewer.UI;

/// <summary>
/// Builds an FsmView from the Validation.FsmRepository indexes.
/// </summary>
public sealed class RepositoryFsmViewBuilder : IFsmViewBuilder
{
    private readonly FsmRepository _repo;
    public RepositoryFsmViewBuilder(FsmRepository repo) => _repo = repo ?? throw new ArgumentNullException(nameof(repo));

    public FsmView Build(State root, string title = "FSM (validated)")
        => throw new NotSupportedException("Use BuildFromRepository(title).");

    public FsmView BuildFromRepository(string title = "FSM")
    {
        if (_repo.RootState is null)
            throw new InvalidOperationException("Repository has no RootState (INITIAL).");

        // 1) Collect ALL top-level roots (ParentId == null), not just initial
        //    Note: repo.RawStates contains states with ParentId == null.
        var topLevel = _repo.RawStates.Values.ToList();

        // 2) Traverse each root to gather all states
        var allStates = new Dictionary<string, RawState>(StringComparer.Ordinal);
        foreach (var root in topLevel)
            CollectStates(root, allStates);

        // 3) Create views
        var viewById = new Dictionary<string, StateView>(StringComparer.Ordinal);
        foreach (var rs in allStates.Values)
        {
            viewById[rs.Id] = new StateView
            {
                Identifier = rs.Id,
                DisplayName = rs.Name,
                IsCompound = rs.type == StateType.COMPOUND
            };
        }

        // 4) Attach state actions (ENTRY/DO/EXIT)
        foreach (var rs in allStates.Values)
        {
            if (!_repo.Actions.TryGetValue(rs.Id, out var acts)) continue;
            var sv = viewById[rs.Id];
            foreach (var a in acts)
            {
                switch (a.Type)
                {
                    case ActionType.ENTRY_ACTION: sv.EntryActions.Add(a.Description); break;
                    case ActionType.DO_ACTION:    sv.DoActions.Add(a.Description);    break;
                    case ActionType.EXIT_ACTION:  sv.ExitActions.Add(a.Description);  break;
                }
            }
        }

        // 5) Wire parent/children
        var rootsForView = new List<StateView>();
        foreach (var rs in topLevel)
        {
            // Skip pseudo states from RootStates
            if (rs.type is StateType.INITIAL or StateType.FINAL) continue;
            if (viewById.TryGetValue(rs.Id, out var v)) rootsForView.Add(v);
        }
        foreach (var rs in allStates.Values)
        {
            if (rs.ParentId is null) continue;
            if (viewById.TryGetValue(rs.ParentId, out var parentV))
                parentV.Children.Add(viewById[rs.Id]);
        }

        // 6) Build transitions
        var topLevelTransitions = new List<TransitionView>();
        foreach (var rs in allStates.Values)
        {
            if (!_repo.SourceTransitions.TryGetValue(rs.Id, out var outgoing)) continue;

            var isTopLevelSource = rs.ParentId is null; // initial/compound/final are top-level
            var fromView = viewById[rs.Id];

            foreach (var t in outgoing)
            {
                var toName = viewById.TryGetValue(t.DestinationStateId, out var toV)
                    ? toV.DisplayName
                    : t.DestinationStateId;

                // Map trigger to description (fallback to id)
                string? triggerText = null;
                if (t.TriggerId is { } trigId && _repo.Triggers.TryGetValue(trigId, out var trig))
                    triggerText = trig.Description;
                else
                    triggerText = t.TriggerId;

                var tv = new TransitionView
                {
                    FromIdentifier = t.SourceStateId,
                    ToIdentifier = t.DestinationStateId,
                    ToDisplayName = toName,
                    TriggerIdentifier = triggerText,
                    GuardCondition = t.GuardCondition
                };

                // Transition effects (TRANSITION_ACTION) keyed by transition id
                if (_repo.Actions.TryGetValue(t.Id, out var tActs))
                    foreach (var a in tActs.Where(x => x.Type == ActionType.TRANSITION_ACTION))
                        tv.TransitionActions.Add(a.Description);

                // Compound/root transitions → show as top-level lines
                if (isTopLevelSource && (rs.type is StateType.INITIAL or StateType.COMPOUND))
                    topLevelTransitions.Add(tv);
                else
                    fromView.Outgoing.Add(tv);
            }
        }

        // 7) Initial & Final
        StateView? initial = null, final = null;
        foreach (var rs in allStates.Values)
        {
            if (rs.type == StateType.INITIAL) initial = viewById[rs.Id];
            if (rs.type == StateType.FINAL)   final   = viewById[rs.Id];
        }

        // 8) Assemble
        var view = new FsmView
        {
            Title = title,
            Initial = initial,
            Final = final
        };
        view.RootStates.AddRange(rootsForView);

        // Order: initial transitions first, then others (e.g., powered -> final)
        topLevelTransitions = topLevelTransitions
            .OrderBy(t => t.FromIdentifier == _repo.RootState!.Id ? 0 : 1)
            .ToList();

        view.TopLevelTransitions.AddRange(topLevelTransitions);
        return view;
    }

    private void CollectStates(RawState root, IDictionary<string, RawState> result)
    {
        var stack = new Stack<RawState>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        stack.Push(root);

        while (stack.Count > 0)
        {
            var s = stack.Pop();
            if (!seen.Add(s.Id)) continue;

            result[s.Id] = s;

            if (_repo.RawChildren.TryGetValue(s.Id, out var kids))
            {
                for (int i = kids.Count - 1; i >= 0; i--)
                    stack.Push(kids[i]);
            }
        }
    }
}
