using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
    public class CompoundStateRule : BaseValidationRule
    {
        public override string RuleName => "CompoundStateValidation";

        public override bool ShouldValidate(IFsmElement element) => element is State state && state.Type == StateType.COMPOUND;

        public override IEnumerable<string> Validate(State state)
        {
            if (state.Children.Count == 0)
            {
                return new[] { $"Compound state '{state.Identifier}' must have at least one child state" };
            }

            return Enumerable.Empty<string>();
        }
    }

}
