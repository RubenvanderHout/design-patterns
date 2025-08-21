using UmlViewer.UI.ModelView;
namespace UmlViewer.UI;

public interface IRenderer
{
    string Render(FsmView view);
    void VisitState(StateView state, int depth);
    void VisitTransition(TransitionView transition, int depth);
}
