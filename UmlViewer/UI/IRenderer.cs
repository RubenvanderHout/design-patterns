using UmlViewer.UI.ModelView;
namespace UmlViewer.UI;

public interface IRenderer
{
    string Render(FsmView view);
}
