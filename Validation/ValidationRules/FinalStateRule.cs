using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
    public class FinalStateRule : BaseValidationRule
    {
        public override string RuleName => "FinalStateValidation";

        public override bool ShouldValidate(IFsmElement element) => element is State state && state.Type == StateType.FINAL;

        public override IEnumerable<string> Validate(State state)
        {
            var errors = new List<string>();

            if (state.SourceTransitions.Count > 0)
            {
                errors.Add($"Final state '{state.Identifier}' must have no outgoing transitions, but has {state.SourceTransitions.Count}");
            }

            if (state.DestinationTransitions.Count == 0)
            {
                errors.Add($"Final state '{state.Identifier}' must have at least one incoming transition, but has none");
            }

            return errors;
        }
    }
}
