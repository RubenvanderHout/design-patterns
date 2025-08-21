using UmlViewer.UI.ModelView;
using Validation;

namespace UmlViewer.UI;

public interface IFsmViewBuilder
{
    FsmView Build(State root, string title = "FSM (validated)");
}
