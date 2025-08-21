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
        _sb.AppendLine();

        if (view.Initial is not null)
        {
            _sb.AppendLine($"   O Initial state ({view.Initial.DisplayName})");
            _sb.AppendLine();
        }

        foreach (var t in view.TopLevelTransitions)
            VisitTransition(t, 0);

        foreach (var root in view.RootStates)
            VisitState(root, 0);

        if (view.Final is not null)
        {
            _sb.AppendLine();
            _sb.AppendLine($"   (O) Final state ({view.Final.DisplayName})");
        }

        return _sb.ToString();
    }

    public void VisitState(StateView s, int depth)
    {
        var indent = new string(' ', depth * 3);

        if (s.IsCompound)
        {
            _sb.AppendLine($"{indent}======================================================================");
            _sb.AppendLine($"{indent}|| Compound state: {s.DisplayName}");
            _sb.AppendLine($"{indent}----------------------------------------------------------------------");
            _sb.AppendLine();

            foreach (var child in s.Children)
                VisitState(child, depth + 1);

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
                VisitTransition(tr, depth);
        }
        _sb.AppendLine();
    }

    public void VisitTransition(TransitionView t, int depth)
    {
        var indent = new string(' ', depth * 3);
        var trigger = t.TriggerIdentifier is null ? "" : t.TriggerIdentifier.Replace('_', ' ');
        var guard = string.IsNullOrEmpty(t.GuardCondition) ? "" : $" [{t.GuardCondition}]";
        var effect = t.TransitionActions.Count == 0 ? "" : $" / {string.Join("; ", t.TransitionActions)}";
        _sb.AppendLine($"{indent}   ---{trigger}{guard}{effect}---> {t.ToDisplayName}");
    }
}
