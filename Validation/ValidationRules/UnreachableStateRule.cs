using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
    public class UnreachableStateRule : BaseValidationRule
    {
        public override string RuleName => "UnreachableStateRule";

        public override bool ShouldValidate(IFsmElement element) => element is State state && state.Type == StateType.SIMPLE;

        public override IEnumerable<string> Validate(State state)
        {
            var errors = new List<string>();

            var noIncomingTransitons = state.DestinationTransitions.Count == 0;
            var hasLeavingTransitions = state.SourceTransitions.Count >= 1;
            if (noIncomingTransitons && hasLeavingTransitions)
            {
                errors.Add($"State '{state.Identifier}' is Unreachable.");
            }

            return errors;
        }
    }
}
