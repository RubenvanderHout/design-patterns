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
    public sealed class FsmRoot(string title, List<State> rootStates, List<State> allStates)
    {
        public string Title { get; init; } = title;
        public List<State> RootStates { get; } = rootStates;
        public List<State> AllStates { get; } = allStates;
    }

    public sealed class FsmRuleParser
    {
        public FsmRepository Repo { get; }
        public FsmRoot FsmRoot { get; } 
        private readonly List<IValidtionRuleVistor> _rules;

        public FsmRuleParser(IEnumerable<IValidtionRuleVistor> rules, FsmDto dto)
        {
            _rules = [.. rules];
            Repo = new FsmRepository(dto);
            FsmRoot = BuildComposites();
        }

        private FsmRoot BuildComposites()
        {
            var parents = Repo.RootStates;

            foreach (var parent in parents)
            {
                parent.Children = Repo.ChildStates.GetValueOrDefault(parent.Identifier, []);
            }

            throw new NotImplementedException();
        }

    }
}
