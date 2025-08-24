using IO.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using Validation.ValidationRules;

namespace Validation
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public sealed class FsmRuleParser
    {
        public FsmRepository Repo { get; }
        private ValidationVisitor RulesValidator { get; }

        public FsmRuleParser(IRuleComponent rulesRoot, FsmDto dto)
        {
            Repo = new FsmRepository(dto);
            RulesValidator = new ValidationVisitor(rulesRoot);
        }

        public ValidationResult Validate()
        {
            foreach (var state in Repo.AllStates.Values)
            {
                state.Accept(RulesValidator);
            }

            return new ValidationResult
            {
                IsValid = RulesValidator.IsValid,
                Errors = [.. RulesValidator.Errors]
            };
        }

    }
}
