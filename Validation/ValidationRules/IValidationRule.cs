using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
    public interface IValidtionRuleVistor
    {
        void VisitState(State state);
        void VisitTransition(Transition transition);
        void VisitAction(Action action);
        void VisitTrigger(Trigger trigger);
    }
}
