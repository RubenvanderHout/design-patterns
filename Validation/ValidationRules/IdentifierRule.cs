using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
    public class IdentifierRule : BaseValidationRule
    {
        public override string RuleName => "IdentifierRule";

        public override bool ShouldValidate(IFsmElement element) => element is IFsmElement;

        private static readonly Regex IdentifierRegex = new(@"[a-zA-Z]+\S*", RegexOptions.Compiled);
        private static IEnumerable<string> CheckIdentifier(FsmNode node)
        {
            var errors = new List<string>();

            if (!IdentifierRegex.IsMatch(node.Identifier))
                errors.Add($"Invalid identifier '{node.Identifier}'. Must be [a-zA-Z]+\\S*");

            return errors;
        }

        public override IEnumerable<string> Validate(State state) => CheckIdentifier(state);
        public override IEnumerable<string> Validate(Transition transition) => CheckIdentifier(transition);
        public override IEnumerable<string> Validate(Trigger trigger) => CheckIdentifier(trigger);
        public override IEnumerable<string> Validate(Action action) => CheckIdentifier(action);
    }
}
