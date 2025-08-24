using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
    public static class ValidationRuleBuilder
    {
        // public static List<IValidationRule> GiveAllValidationRules()
        // {
        //     var list = new List<IValidationRule>()
        //     {
        //         new CompoundStateRule(),
        //         new FinalStateRule(),
        //         new IdentifierRule(),
        //         new InitialStateRule(),
        //         new NonDeterministicTransitionsRule(),
        //         new TransitionRule(),
        //         new TransitionToCompoundStateRule(),
        //         new UnreachableStateRule(),
        //     };

        //     return list;
        // }

        public static IRuleComponent BuildCompleteComposite()
        {
            var stateRules = new RuleGroup("State rules")
                .Add(new RuleLeaf(new IdentifierRule()))
                .Add(new RuleLeaf(new InitialStateRule()))
                .Add(new RuleLeaf(new FinalStateRule()))
                .Add(new RuleLeaf(new CompoundStateRule()))
                .Add(new RuleLeaf(new UnreachableStateRule()));

            var transitionRules = new RuleGroup("Transition rules")
                .Add(new RuleLeaf(new TransitionRule()))
                .Add(new RuleLeaf(new TransitionToCompoundStateRule()))
                .Add(new RuleLeaf(new NonDeterministicTransitionsRule()));

            return new RuleGroup("All rules")
                .Add(stateRules)
                .Add(transitionRules);
        }
    }
}
