using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
    public class TransitionRule : BaseValidationRule
    {
        public override string RuleName => "TransitionValidation";

        public override bool ShouldValidate(IFsmElement element) => element is Transition;

        public override IEnumerable<string> Validate(Transition transition)
        {
            var errors = new List<string>();

            if (transition.SourceState.Type == StateType.FINAL)
            {
                errors.Add($"Transition '{transition.Identifier}' cannot originate from a final state");
            }

            return errors;
        }
    }
}
