using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validation.ValidationRules;

namespace Validation
{
    public sealed record FsmDefinition(
       IReadOnlyDictionary<string, State> States,
       IReadOnlyDictionary<string, Trigger> Triggers,
       IReadOnlyDictionary<string, ActionDef> Actions,
       IReadOnlyDictionary<string, Transition> Transitions
    );

    public sealed class FsmRuleParser
    {
        private readonly List<IValidationRule> _rules;

        private readonly Dictionary<string, State> _states = [];
        private readonly Dictionary<string, Trigger> _triggers = [];
        private readonly Dictionary<string, ActionDef> _actions = [];
        private readonly Dictionary<string, Transition> _transitions = [];

        public FsmRuleParser(IEnumerable<IValidationRule> rules)
        {
            _rules = [.. rules];
        }

        public FsmDefinition Build()
        {
            return new FsmDefinition(_states, _triggers, _actions, _transitions);
        }

        private void ApplyRules(object obj)
        {
            var snapshot = Build(); // current definition so far
            foreach (var rule in _rules)
                rule.Apply(obj, snapshot);
        }

        public State AddState(string identifier, string parent, string name, StateType type)
        {
            var state = new State(identifier, parent == "_" ? null : parent, name, type);
            ApplyRules(state);
            _states[state.Identifier] = state;
            return state;
        }

        public Trigger AddTrigger(string identifier, string description)
        {
            var trigger = new Trigger(identifier, description);
            ApplyRules(trigger);
            _triggers[trigger.Identifier] = trigger;
            return trigger;
        }

        public ActionDef AddAction(string ownerIdentifier, string description, ActionType type)
        {
            var action = new ActionDef(ownerIdentifier, description, type);
            ApplyRules(action);
            _actions[ownerIdentifier + ":" + type] = action;
            return action;
        }

        public Transition AddTransition(string identifier, string source, string destination, string? trigger, string guard)
        {
            var transition = new Transition(identifier, source, destination, trigger, guard);
            ApplyRules(transition);
            _transitions[identifier] = transition;
            return transition;
        }
    }
}
