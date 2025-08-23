using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
    public class InitialStateRule : BaseValidationRule
    {
        public override string RuleName => "InitialStateValidation";

        public override bool ShouldValidate(IFsmElement element) => element is State state && state.Type == StateType.INITIAL;

        public override IEnumerable<string> Validate(State state)
        {
            var errors = new List<string>();

            if (state.SourceTransitions.Count != 1)
            {
                errors.Add($"Initial state '{state.Identifier}' must have exactly one outgoing transition, but has {state.SourceTransitions.Count}");
            }

            if (state.DestinationTransitions.Count > 0)
            {
                errors.Add($"Initial state '{state.Identifier}' must have no incoming transitions, but has {state.DestinationTransitions.Count}");
            }

            return errors;
        }
    }
}
