using UmlViewer.UI.ModelView;
using Validation;

namespace UmlViewer.UI;

/// <summary>
/// Builds an FsmView from Validation.FsmRepository
/// </summary>
public sealed class RepositoryFsmViewBuilder : IFsmViewBuilder
{
    public FsmView Build(FsmRepository repo, string title = "FSM")
    {
        if (repo == null) throw new ArgumentNullException(nameof(repo));

        var allStates = CollectAllStates(repo);

        var viewById = MapStates(allStates);

        AttachActions(allStates, viewById);

        var rootViews = BuildRootsForView(repo, allStates, viewById);

        var topLevelTransitions = BuildTransitions(repo, allStates, viewById);

        var (initial, final) = FindInitialAndFinal(allStates, viewById);

        var orderedTop = OrderTopLevelTransitions(topLevelTransitions, initial.Identifier);

        var view = FsmView.Create(title, initial, final, rootViews, orderedTop);
        return view;
    }

    private static Dictionary<string, State> CollectAllStates(FsmRepository repo)
    {
        var result = new Dictionary<string, State>(StringComparer.Ordinal);
        foreach (var kv in repo.RootStates)
            CollectFromRoot(repo, kv.Value, result);
        if (result.Count == 0)
            throw new InvalidOperationException("Repository has no top-level states.");
        return result;
    }

    private static void CollectFromRoot(FsmRepository repo, State root, IDictionary<string, State> result)
    {
        var stack = new Stack<State>();
        var seen  = new HashSet<string>(StringComparer.Ordinal);
        stack.Push(root);

        while (stack.Count > 0)
        {
            var s = stack.Pop();
            if (!seen.Add(s.Identifier)) continue;

            result[s.Identifier] = s;

            if (repo.ChildStates.TryGetValue(s.Identifier, out var kids))
                for (int i = kids.Count - 1; i >= 0; i--)
                    stack.Push(kids[i]);
        }
    }

    private static Dictionary<string, StateView> MapStates(Dictionary<string, State> allStates)
    {
        var viewById = new Dictionary<string, StateView>(allStates.Count, StringComparer.Ordinal);
        foreach (var s in allStates.Values)
        {
            viewById[s.Identifier] = new StateView
            {
                Identifier  = s.Identifier,
                DisplayName = s.Name,
                IsCompound  = s.Type == StateType.COMPOUND
            };
        }
        return viewById;
    }

    private static void AttachActions(Dictionary<string, State> allStates, Dictionary<string, StateView> viewById)
    {
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
    }

    private static List<StateView> BuildRootsForView(FsmRepository repo, Dictionary<string, State> allStates, Dictionary<string, StateView> viewById)
    {
        var rootsForView = new List<StateView>();

        foreach (var root in repo.RootStates.Values)
        {
            if (root.Type is StateType.INITIAL or StateType.FINAL) continue;
            rootsForView.Add(viewById[root.Identifier]);
        }

        foreach (var parent in allStates.Values)
        {
            if (repo.ChildStates.TryGetValue(parent.Identifier, out var kids))
            {
                var pv = viewById[parent.Identifier];
                foreach (var child in kids)
                    pv.Children.Add(viewById[child.Identifier]);
            }
        }

        return rootsForView;
    }

    private static List<TransitionView> BuildTransitions(FsmRepository repo, Dictionary<string, State> allStates, Dictionary<string, StateView> viewById)
    {
        var topLevelTransitions = new List<TransitionView>();

        foreach (var s in allStates.Values)
        {
            if (!repo.SourceTransitions.TryGetValue(s.Identifier, out var outgoing))
                continue;

            var isTopLevelSource = s.ParentId is null;
            var fromView = viewById[s.Identifier];

            foreach (var t in outgoing)
            {
                var toName = viewById.TryGetValue(t.DestinationState.Identifier, out var toV)
                    ? toV.DisplayName
                    : t.DestinationState.Identifier;

                var tv = new TransitionView
                {
                    FromIdentifier    = t.SourceState.Identifier,
                    ToIdentifier      = t.DestinationState.Identifier,
                    ToDisplayName     = toName,
                    TriggerIdentifier = t.Trigger?.Description ?? string.Empty,
                    GuardCondition    = t.GuardCondition
                };

                if (t.Action is { } eff && eff.Type == ActionType.TRANSITION_ACTION)
                    tv.TransitionActions.Add(eff.Description);

                if (isTopLevelSource && (s.Type is StateType.INITIAL or StateType.COMPOUND))
                    topLevelTransitions.Add(tv);
                else
                    fromView.Outgoing.Add(tv);
            }
        }

        return topLevelTransitions;
    }

    private static (StateView initial, StateView final) FindInitialAndFinal(Dictionary<string, State> allStates, Dictionary<string, StateView> viewById)
    {
        StateView? initial = null, final = null;
        foreach (var s in allStates.Values)
        {
            if (s.Type == StateType.INITIAL) initial = viewById[s.Identifier];
            if (s.Type == StateType.FINAL)   final   = viewById[s.Identifier];
        }
        if (initial is null || final is null)
            throw new InvalidOperationException("FSM must have an initial and final state.");
        return (initial, final);
    }

    private static List<TransitionView> OrderTopLevelTransitions(List<TransitionView> top, string initialId)
        => top.OrderBy(t => t.FromIdentifier == initialId ? 0 : 1).ToList();
}