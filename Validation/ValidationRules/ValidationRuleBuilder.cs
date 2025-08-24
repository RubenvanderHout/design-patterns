using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
    public static class ValidationRuleBuilder
    {
        public static List<IValidationRule> GiveAllValidationRules()
        {
            var list = new List<IValidationRule>()
            {
                new CompoundStateRule(),
                new FinalStateRule(),
                new IdentifierRule(),
                new InitialStateRule(),
                new NonDeterministicTransitionsRule(),
                new TransitionRule(),
                new TransitionToCompoundStateRule(),
                new UnreachableStateRule(),
            };

            return list;
        }
    }
}
