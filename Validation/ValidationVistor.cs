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
        private readonly IRuleComponent _root;     
        private readonly List<string> _errors = new();

        public ValidationVisitor(IRuleComponent root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public IReadOnlyList<string> Errors => _errors.AsReadOnly();
        public bool IsValid => _errors.Count == 0;

        public void Visit(State s)      => _errors.AddRange(_root.Validate(s));

        public void Visit(Transition t) => _errors.AddRange(_root.Validate(t));

        public void Visit(Action a)      => _errors.AddRange(_root.Validate(a));

        public void Visit(Trigger t)     => _errors.AddRange(_root.Validate(t));
    }
}
