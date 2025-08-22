using UmlViewer.UI.ModelView;
using Validation;

namespace UmlViewer.UI;

/// <summary>
/// Builds an FsmView from Validation.FsmRepository (new public API).
/// </summary>
public sealed class RepositoryFsmViewBuilder : IFsmViewBuilder
{
    private readonly FsmRepository _repo;
    public RepositoryFsmViewBuilder(FsmRepository repo) =>
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));

    public FsmView Build(State root, string title = "FSM (validated)") =>
        throw new NotSupportedException("Use BuildFromRepository(title).");

    public FsmView BuildFromRepository(string title = "FSM")
    {
        // 1) Top-level roots
        var topLevel = _repo.RootStates;
        if (topLevel.Count == 0)
            throw new InvalidOperationException("Repository has no top-level states.");

        // 2) Collect reachable states from all roots using ChildStates index
        var allStates = new Dictionary<string, State>(StringComparer.Ordinal);
        foreach (var root in topLevel)
            CollectStates(root, allStates);

        // 3) Map to StateView
        var viewById = new Dictionary<string, StateView>(allStates.Count, StringComparer.Ordinal);
        foreach (var s in allStates.Values)
        {
            viewById[s.Identifier] = new StateView
            {
                Identifier = s.Identifier,
                DisplayName = s.Name,
                IsCompound = s.Type == StateType.COMPOUND
            };
        }

        // 4) Attach state actions (ENTRY/DO/EXIT) from State.Actions
        foreach (var s in allStates.Values)
        {
            var sv = viewById[s.Identifier];
            foreach (var a in s.Actions)
            {
                switch (a.Type)
                {
                    case ActionType.ENTRY_ACTION: sv.EntryActions.Add(a.Description); break;
                    case ActionType.DO_ACTION:    sv.DoActions.Add(a.Description);    break;
                    case ActionType.EXIT_ACTION:  sv.ExitActions.Add(a.Description);  break;
                }
            }
        }

        // 5) Wire parent/children using ChildStates
        var rootsForView = new List<StateView>();
        foreach (var root in topLevel)
        {
            if (root.Type is StateType.INITIAL or StateType.FINAL) continue;
            rootsForView.Add(viewById[root.Identifier]);
        }

        foreach (var parent in allStates.Values)
        {
            if (_repo.ChildStates.TryGetValue(parent.Identifier, out var kids))
            {
                var pv = viewById[parent.Identifier];
                foreach (var child in kids)
                    pv.Children.Add(viewById[child.Identifier]);
            }
        }

        // 6) Build transitions from SourceTransitions
        var topLevelTransitions = new List<TransitionView>();
        foreach (var s in allStates.Values)
        {
            if (!_repo.SourceTransitions.TryGetValue(s.Identifier, out var outgoing)) continue;

            var isTopLevelSource = s.ParentId is null;
            var fromView = viewById[s.Identifier];

            foreach (var t in outgoing)
            {
                var toName = viewById.TryGetValue(t.DestinationStateId, out var toV)
                    ? toV.DisplayName
                    : t.DestinationStateId;

                var triggerText = t.Trigger?.Description ?? string.Empty;

                var tv = new TransitionView
                {
                    FromIdentifier = t.SourceStateId,
                    ToIdentifier   = t.DestinationStateId,
                    ToDisplayName  = toName,
                    TriggerIdentifier = triggerText,
                    GuardCondition = t.GuardCondition
                };

                // Transition effect: repo’s Transition holds a single Action (effect) already
                if (t.Action is { } eff && eff.Type == ActionType.TRANSITION_ACTION)
                    tv.TransitionActions.Add(eff.Description);

                // Initial/Compound top-level transitions are rendered outside blocks
                if (isTopLevelSource && (s.Type is StateType.INITIAL or StateType.COMPOUND))
                    topLevelTransitions.Add(tv);
                else
                    fromView.Outgoing.Add(tv);
            }
        }

        // 7) Initial & Final
        StateView? initial = null, final = null;
        foreach (var s in allStates.Values)
        {
            if (s.Type == StateType.INITIAL) initial = viewById[s.Identifier];
            if (s.Type == StateType.FINAL)   final   = viewById[s.Identifier];
        }
        if (initial is null || final is null)
            throw new InvalidOperationException("FSM must have an initial and final state.");

        // 8) Assemble + order top-level transitions: initial first
        var view = new FsmView
        {
            Title = title,
            Initial = initial,
            Final = final
        };
        view.RootStates.AddRange(rootsForView);

        var initialId = allStates.Values.First(s => s.Type == StateType.INITIAL).Identifier;
        topLevelTransitions = topLevelTransitions
            .OrderBy(t => t.FromIdentifier == initialId ? 0 : 1)
            .ToList();

        view.TopLevelTransitions.AddRange(topLevelTransitions);
        return view;
    }

    private void CollectStates(State root, IDictionary<string, State> result)
    {
        var stack = new Stack<State>();
        var seen  = new HashSet<string>(StringComparer.Ordinal);
        stack.Push(root);

        while (stack.Count > 0)
        {
            var s = stack.Pop();
            if (!seen.Add(s.Identifier)) continue;

            result[s.Identifier] = s;

            if (_repo.ChildStates.TryGetValue(s.Identifier, out var kids))
            {
                for (int i = kids.Count - 1; i >= 0; i--)
                    stack.Push(kids[i]);
            }
        }
    }
}
