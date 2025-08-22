using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{

    public interface IValidationRule
    {
        string RuleName { get; }
        bool ShouldValidate(IFsmElement element);
        IEnumerable<string> Validate(State state);
        IEnumerable<string> Validate(Transition transition);
        IEnumerable<string> Validate(Trigger trigger);
        IEnumerable<string> Validate(Action action);
    }

    public abstract class BaseValidationRule : IValidationRule
    {
        public abstract string RuleName { get; }

        public virtual bool ShouldValidate(IFsmElement element) => true;

        public virtual IEnumerable<string> Validate(State state) => [];
        public virtual IEnumerable<string> Validate(Transition transition) => [];
        public virtual IEnumerable<string> Validate(Trigger trigger) => [];
        public virtual IEnumerable<string> Validate(Action action) => [];
    }

}
