using UmlViewer.UI.ModelView;
using Validation;

namespace UmlViewer.UI;

// TEMP: returns an empty view until the real builder is plugged in.
public sealed class StubFsmViewBuilder : IFsmViewBuilder
{
    public FsmView Build(State root, string title = "FSM (validated)") => new() { Title = title };
}
