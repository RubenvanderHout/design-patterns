using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
    public sealed class IdentifierRule : IValidationRule
    {
        private static readonly Regex IdentifierRegex = new(@"[a-zA-Z]+\S*", RegexOptions.Compiled);

        public void Apply(object obj, FsmDefinition current)
        {
            string? id = obj switch
            {
                State s => s.Identifier,
                Trigger t => t.Identifier,
                ActionDef a => a.OwnerIdentifier, // optional: could validate action owner differently
                Transition tr => tr.Identifier,
                _ => null
            };

            if (id != null && !IdentifierRegex.IsMatch(id))
                throw new ArgumentException($"Invalid identifier '{id}'. Must be [a-zA-Z]+\\S*");
        }
    
    }
}
