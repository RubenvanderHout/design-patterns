using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
    public class TransitionToCompoundStateRule : BaseValidationRule
    {
        public override string RuleName => "TransitionToCompoundStateValidation";

        public override bool ShouldValidate(IFsmElement element) => element is Transition transition &&
                                                                    transition.DestinationState.Type == StateType.COMPOUND;
        public override IEnumerable<string> Validate(Transition transition)
        {
            return new[] { $"Transition '{transition.Identifier}' cannot end at a compound state '{transition.DestinationState.Identifier}'" };
        }
    }
}
