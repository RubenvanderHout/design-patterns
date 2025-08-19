using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validation.ValidationRules
{
   public interface IValidationRule
   {
        /// <summary>
        /// Validates a object.
        /// Throws Argumentexception if the rule is invalid
        /// </summary>
        /// 
        void Apply(object obj, FsmDefinition current);
    }
}
