using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
    public class NonDeterministicTransitionsRule : BaseValidationRule
    {
        public override string RuleName => "NonDeterministicTransitionsRule";

        public override bool ShouldValidate(IFsmElement element) => element is State state && state.SourceTransitions.Count > 1;

        public override IEnumerable<string> Validate(State state)
        {
            var errors = new List<string>();

            var transitionTexts =
                from trans in state.SourceTransitions
                select $"{trans.Trigger?.Description ?? ""}{trans.GuardCondition}";

            if (transitionTexts.Contains(String.Empty) && transitionTexts.Any())
            {
                errors.Add(
                    $"State {state.Identifier} has a transaction an automatic transaction " +
                    $"this makes other transactions redundant please remove the other transactions or give the automatic transaction a trigger"
                );
            }
            
            var duplicateCount = transitionTexts
                .GroupBy(text => text)
                .Where(group => group.Count() > 1)
                .Sum(group => group.Count());

            if (duplicateCount > 0)
            {
                errors.Add($"State {state.Identifier} has non deterministic transactions coming from them please make every case navigational and unique");
            }

            return errors;
        }
    }
}
