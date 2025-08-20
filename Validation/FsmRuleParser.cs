using IO.DTO;
using System;
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
        private readonly List<IValidationRule> _rules;
        private readonly List<Transition> _transitions;

        // Primary indexes
        private readonly Dictionary<string, State> _states;
        private readonly Dictionary<string, Trigger> _triggers;
        private readonly Dictionary<string, List<Action>> _actions;

        // Secondary indexes
        private readonly Dictionary<string, Range> _sourceStateTransitions = new();
        private readonly Dictionary<string, Range> _destinationTransitions = new();
 

        public FsmRuleParser(IEnumerable<IValidationRule> rules, FsmDto dto)
        {
            _rules = [.. rules];
            _states = new Dictionary<string, State>();
            _transitions = new List<Transition>();
            _triggers = new Dictionary<string, Trigger>();
            _actions = new Dictionary<string, List<Action>>();
        }

        private void BuildTriggers(in FsmDto dto)
        {
            foreach (var triItem in dto.Triggers)
            {
                var trigger = new Trigger(triItem.Identifier, triItem.Description);
                _triggers.Add(trigger.Identifier, trigger);
            }
        }
        private void BuildActions(in FsmDto dto)
        {
            foreach (var aItem in dto.Actions)
            {
                var actionType = MatchDtoToActionType(aItem.Type);
                var action = new Action(aItem.Identifier, aItem.Description, actionType);

                if (_actions.TryGetValue(aItem.Identifier, out var list))
                {
                    list.Add(action);
                }
                else
                {
                    var listNew = new List<Action>
                    {
                        action
                    };
                    _actions.Add(action.Identifier, listNew);
                }
            }
        }

        public void BuildTransitions(FsmDto dto)
        {
            foreach (var tItem in dto.Transitions)
            {
                // Transitions always have one element 
                var action = GetOrThrow(_actions, tItem.Identifier, "Action")[0];

                Trigger? trigger = tItem.TriggerIdentifier == null
                    ? null
                    : GetOrThrow(_triggers, tItem.TriggerIdentifier, "Trigger");

                var transition = new Transition(
                    tItem.Identifier,
                    tItem.SourceStateIdentifier, // Used as key for filtering later
                    tItem.DestinationStateIdentifier, // Used as key for filtering later
                    trigger,
                    action,
                    tItem.GuardCondition
                );

                _transitions.Add(transition);
            }
        }


        public void BuildTransitionsStateTypeIndex(List<Transition> transitions)
        {
            foreach (var transition in transitions)
            {
                //transition
            }
        }


        public void BuildSourceTransitions(FsmDto dto)
        { 
            _transitions.Sort((a, b) => a.SourceStateIdentifier.CompareTo(b.SourceStateIdentifier));


        }


        public void BuildDestinatnioTransitions(FsmDto dto)
        {

        }

        //private void BuildIndex(
        //    List<Transition> trs,
        //    Func<Transition, string> keySelector,
        //    Dictionary<string, Range> index
        //)
        //{
        //    // Clear already filled indexes
        //    index.Clear();

        //    int start = 0;
        //    while(start < trs.Count)
        //    {   
        //        // Func delegate for selecting property
        //        var key = keySelector(trs[start]);
        //        int end = start + 1;
        //        while (end < trs.Count && keySelector(trs[end]) == key)
        //            end++;
        //        index
        //    }
        //}





        private Dictionary<string, State> BuildStructure(in FsmDto dto)
        {
            // Index triggers
            foreach (var triItem in dto.Triggers)
            {
                var trigger = new Trigger(triItem.Identifier, triItem.Description);
                _triggers.Add(trigger.Identifier, trigger);
            }

            // Index Actions
            foreach (var aItem in dto.Actions)
            {
                var actionType = MatchDtoToActionType(aItem.Type);
                var action = new Action(aItem.Identifier, aItem.Description, actionType);

                if (_actions.TryGetValue(aItem.Identifier, out var list))
                {
                    list.Add(action);
                }
                else
                {
                    var listNew = new List<Action>
                    {
                        action
                    };
                    _actions.Add(action.Identifier, listNew);
                }
            }

            // Index Transitions
            foreach (var tItem in dto.Transitions)
            {
                var source = GetOrThrow(_states, tItem.SourceStateIdentifier, "Source state");
                var destination = GetOrThrow(_states, tItem.DestinationStateIdentifier, "Destination state");
                var action = GetOrThrow(_actions, tItem.Identifier, "Action")[0];

                Trigger? trigger = tItem.TriggerIdentifier != null
                    ? GetOrThrow(_triggers, tItem.TriggerIdentifier, "Trigger")
                    : null;


                // Do the 

                var transition = new Transition(
                    tItem.Identifier,
                    source,
                    destination,
                    trigger,
                    action,
                    tItem.GuardCondition
                );

                _transitions.Add(transition.Identifier, transition);
            }


            // Index States
            foreach (var sItem in dto.States)
            {
                var stateType = MatchDtoToStateType(sItem.Type);

                var state = new State(sItem.Identifier, stateType, sItem.Parent);
                _states.Add(state.Identifier, state);
            }

            // Second pass: wire parents & children
            foreach (var state in _states.Values)
            {
                if (state.ParentId != null)
                {
                    var parent = GetOrThrow(_states, state.ParentId, "Parent state");
                    state.AddParent(parent);
                }
            }

        }

        //public FsmDefinition Build()
        //{
        //    return new FsmDefinition(_states, _triggers, _actions, _transitions);
        //}

        //private void ApplyRules(object obj)
        //{
        //    var snapshot = Build(); // current definition so far
        //    foreach (var rule in _rules)
        //        rule.Apply(obj, snapshot);
        //}


        //public State AddState(string identifier, string parent, string name, StateType type)
        //{
        //    var state = new State(identifier, parent == "_" ? null : parent, name, type);
        //    ApplyRules(state);
        //    _states[state.Identifier] = state;
        //    return state;
        //}

        //public Trigger AddTrigger(string identifier, string description)
        //{
        //    var trigger = new Trigger(identifier, description);
        //    ApplyRules(trigger);
        //    _triggers[trigger.Identifier] = trigger;
        //    return trigger;
        //}

        //public ActionDef AddAction(string ownerIdentifier, string description, ActionType type)
        //{
        //    var action = new ActionDef(ownerIdentifier, description, type);
        //    ApplyRules(action);
        //    _actions[ownerIdentifier + ":" + type] = action;
        //    return action;
        //}

        //public Transition AddTransition(string identifier, string source, string destination, string? trigger, string guard)
        //{
        //    var transition = new Transition(identifier, source, destination, trigger, guard);
        //    ApplyRules(transition);
        //    _transitions[identifier] = transition;
        //    return transition;
        //}


        private static Validation.StateType MatchDtoToStateType(IO.DTO.StateType stateType)
        {
            var vst = stateType switch
            {
                IO.DTO.StateType.INITIAL => StateType.INITIAL,
                IO.DTO.StateType.SIMPLE => StateType.SIMPLE,
                IO.DTO.StateType.COMPOUND => StateType.COMPOUND,
                IO.DTO.StateType.FINAL => StateType.FINAL,
                _ => throw new NotImplementedException("Dev error handle the other cases of the DTO"),
            };
            return vst;
        }

        private static Validation.ActionType MatchDtoToActionType(IO.DTO.ActionType actionType)
        {
            var vst = actionType switch
            {
                IO.DTO.ActionType.ENTRY_ACTION => ActionType.ENTRY_ACTION,
                IO.DTO.ActionType.DO_ACTION => ActionType.DO_ACTION,
                IO.DTO.ActionType.EXIT_ACTION => ActionType.EXIT_ACTION,
                IO.DTO.ActionType.TRANSITION_ACTION => ActionType.TRANSITION_ACTION,
                _ => throw new NotImplementedException("Dev error handle the other cases of the DTO"),
            };
            return vst;
        }
    }
}
