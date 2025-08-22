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

            if(state.SourceTransitions.Count == 0 && state.DestinationTransitions.Count > 0 || 
                state.SourceTransitions.Count > 0 && state.DestinationTransitions.Count == 0)
            {
                errors.Add($"State '{state.Identifier}' is Unreachable.");
            }

            return errors;
        }
    }
}
