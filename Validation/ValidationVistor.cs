using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validation.ValidationRules;

namespace Validation
{
    public interface IVisitor
    {
        void Visit(State state);
        void Visit(Transition transition);
        void Visit(Action action);
        void Visit(Trigger trigger);
    }
    public class ValidationVisitor : IVisitor
    {
        private readonly IEnumerable<IValidationRule> _rules;
        private readonly List<string> _errors = new();

        public ValidationVisitor(IEnumerable<IValidationRule> rules)
        {
            _rules = rules;
        }

        public IReadOnlyList<string> Errors => _errors.AsReadOnly();
        public bool IsValid => _errors.Count == 0;

        public void Visit(State state)
        {
            foreach (var rule in _rules.Where(r => r.ShouldValidate(state)))
            {
                _errors.AddRange(rule.Validate(state));
            }

        }

        public void Visit(Transition transition)
        {
            foreach (var rule in _rules.Where(r => r.ShouldValidate(transition)))
            {
                _errors.AddRange(rule.Validate(transition));
            }
        }

        public void Visit(Action action)
        {
            foreach (var rule in _rules.Where(r => r.ShouldValidate(action)))
            {
                _errors.AddRange(rule.Validate(action));
            }
        }

        public void Visit(Trigger trigger)
        {
            foreach (var rule in _rules.Where(r => r.ShouldValidate(trigger)))
            {
                _errors.AddRange(rule.Validate(trigger));
            }
        }
    }
}
