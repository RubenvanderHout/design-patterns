using System.Text;
using UmlViewer.UI.ModelView;
namespace UmlViewer.UI;

public sealed class TextRenderer : IRenderer
{
    private readonly StringBuilder _sb = new();

    public string Render(FsmView view)
    {
        _sb.Clear();
        _sb.AppendLine("######################################################################");
        _sb.AppendLine($"# Diagram: {view.Title}");
        _sb.AppendLine("######################################################################");

        if (view.Initial is not null)
            _sb.AppendLine($"O Initial state ({view.Initial.DisplayName})");

        var initialId = view.Initial?.Identifier;
        var initialTransitions = view.TopLevelTransitions
            .Where(t => initialId != null && t.FromIdentifier == initialId)
            .ToList();
        var otherTopLevel = view.TopLevelTransitions
            .Where(t => initialId == null || t.FromIdentifier != initialId)
            .ToList();

        foreach (var t in initialTransitions)
            DrawTransition(t, 0);

        foreach (var root in view.RootStates)
            DrawState(root, 0);

        foreach (var t in otherTopLevel)
            DrawTransition(t, 0);

        if (view.Final is not null)
            _sb.AppendLine($"(O) Final state ({view.Final.DisplayName})");

        return _sb.ToString();
    }

    private void DrawState(StateView s, int depth)
    {
        var indent = new string(' ', depth * 3);

        if (s.IsCompound)
        {
            _sb.AppendLine($"{indent}======================================================================");
            _sb.AppendLine($"{indent}|| Compound state: {s.DisplayName}");
            _sb.AppendLine($"{indent}----------------------------------------------------------------------");
            _sb.AppendLine();

            foreach (var child in s.Children)
                DrawState(child, depth + 1);

            _sb.AppendLine($"{indent}======================================================================");
        }
        else
        {
            _sb.AppendLine($"{indent}----------------------------------------------------------------------");
            _sb.AppendLine($"{indent}| {s.DisplayName}");
            _sb.AppendLine($"{indent}----------------------------------------------------------------------");
            if (s.EntryActions.Count > 0) _sb.AppendLine($"{indent}| On Entry / {string.Join("; ", s.EntryActions)}");
            if (s.DoActions.Count > 0)    _sb.AppendLine($"{indent}| Do / {string.Join("; ", s.DoActions)}");
            if (s.ExitActions.Count > 0)  _sb.AppendLine($"{indent}| On Exit / {string.Join("; ", s.ExitActions)}");
            _sb.AppendLine($"{indent}----------------------------------------------------------------------");

            foreach (var tr in s.Outgoing)
                DrawTransition(tr, depth);
        }
        _sb.AppendLine();
    }

    private void DrawTransition(TransitionView t, int depth)
    {
        var indent  = new string(' ', depth * 3);
        var trigger = t.TriggerIdentifier ?? "";
        var guard   = string.IsNullOrEmpty(t.GuardCondition) ? "" : $" [{t.GuardCondition}]";
        var effect  = t.TransitionActions.Count == 0 ? "" : $" / {string.Join("; ", t.TransitionActions)}";

        _sb.AppendLine($"{indent}---{trigger}{guard}{effect}---> {t.ToDisplayName}");
    }
}
