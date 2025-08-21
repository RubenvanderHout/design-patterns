using IO.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using Validation.ValidationRules;

namespace Validation
{
 
    public sealed class FsmRuleParser
    {
        private readonly List<IValidtionRuleVistor> _rules;
        private readonly FsmRepository _repo;

        public FsmRuleParser(IEnumerable<IValidtionRuleVistor> rules, FsmRepository repo)
        {
            _repo = repo;
            _rules = [.. rules];

            var result = BuildComposite(_repo.RootState);
        }

        private State BuildComposite(RawState? rootstate)
        {
            //if (rootstate == null)
            //{
            //    throw new InvalidOperationException("Syntax error: Should have an INITIAL state");
            //}

            //// Get 
            ///
            //// Get actions
            //// Get sourceTransitions
            //    // Get Trigger 
            //    // Get Action
            //// Get destinatnionTransitions
            //    // Get Trigger 
            //    // Get Action


            //_repo.Rawchildren.TryGetValue(rootstate.Id, out var children);

            //children
            //    .Select(child => )




            //return new State(rootstate.Id, rootstate.type, );
            throw new NotImplementedException();
        }


    }
}
