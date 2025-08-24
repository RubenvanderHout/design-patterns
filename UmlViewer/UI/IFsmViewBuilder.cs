using UmlViewer.UI.ModelView;
using Validation;

namespace UmlViewer.UI;


public interface IFsmViewBuilder
{
    FsmView Build(FsmRepository repo, string title = "FSM");
}
