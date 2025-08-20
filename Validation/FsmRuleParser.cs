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
            if (rootstate == null)
            {
                throw new InvalidOperationException("Syntax error: Should have an INITIAL state");
            }

            // Get childeren
            // Get actions
            // Get sourceTransitions
                // Get Trigger 
                // Get Action
            // Get destinatnionTransitions
                // Get Trigger 
                // Get Action


            _repo.RawChilderen.TryGetValue(rootstate.Id, out var childeren);

            childeren
                .Select(child => )
      



            return new State(rootstate.Id, rootstate.type, );
        }







        //private void BuildOtherThing(in FsmDto dto)
        //{
        //    foreach (var triItem in dto.Triggers)
        //    {
        //        var trigger = new Trigger(triItem.Identifier, triItem.Description);
        //        _triggers.Add(trigger.Identifier, trigger);
        //    }

        //    foreach (var aItem in dto.Actions)
        //    {
        //        var actionType = MatchDtoToActionType(aItem.Type);
        //        var action = new Action(aItem.Identifier, aItem.Description, actionType);

        //        if (_actions.TryGetValue(aItem.Identifier, out var list))
        //        {
        //            list.Add(action);
        //        }
        //        else
        //        {
        //            var listNew = new List<Action>
        //            {
        //                action
        //            };
        //            _actions.Add(action.Identifier, listNew);
        //        }
        //    }

        //    foreach (var sItem in dto.States)
        //    {
        //        var stateType = MatchDtoToStateType(sItem.Type);
        //        var actions = GetOrThrow(_actions, sItem.Identifier, "Action");

        //        var state = new State(sItem.Identifier, stateType, actions, sItem.Parent);

        //        _states.Add(state.Identifier, state);
        //    }

        //    foreach (var tItem in dto.Transitions)
        //    {
        //        var source = GetOrThrow(_states, tItem.SourceStateIdentifier, "Source state");
        //        var destination = GetOrThrow(_states, tItem.DestinationStateIdentifier, "Destination state");
        //        var action = GetOrThrow(_actions, tItem.Identifier, "Action")[0];

        //        Trigger? trigger = tItem.TriggerIdentifier != null
        //            ? GetOrThrow(_triggers, tItem.TriggerIdentifier, "Trigger")
        //            : null;

        //        if(trigger == null)
        //        {
        //            throw new InvalidOperationException("Triggers should be intialised here");
        //        }

        //        var transition = new Transition(
        //            tItem.Identifier,
        //            source,
        //            destination,
        //            trigger,
        //            action,
        //            tItem.GuardCondition
        //        );

        //        // Setting the Indexes to be used by states later could maybe use slices instead 
        //        if (_sourceStateTransitions.TryGetValue(tItem.SourceStateIdentifier, out var sourcelist))
        //        {
        //            sourcelist.Add(transition);
        //        }
        //        else
        //        {
        //            var listSourceNew = new List<Transition>
        //            {
        //                transition
        //            };
        //            _sourceStateTransitions.Add(action.Identifier, listSourceNew);
        //        }

        //        if (_destinationTransitions.TryGetValue(tItem.DestinationStateIdentifier, out var list))
        //        {
        //            list.Add(transition);
        //        }
        //        else
        //        {
        //            var listDestinationNew = new List<Transition>
        //            {
        //                transition
        //            };
        //            _destinationTransitions.Add(action.Identifier, listDestinationNew);
        //        }

        //        _transitions.Add(transition.Identifier, transition);
        //    }

        //    var parentCache = new Dictionary<string, State>();

        //    // Second pass wire childeren, and transitions to the object
        //    foreach (var state in _states.Values)
        //    {
        //        var sources = GetOrThrow(_sourceStateTransitions, state.Identifier, "Source state");
        //        var destinations = GetOrThrow(_destinationTransitions, state.Identifier, "Destination state");

        //        state.IntialiseSourceTransitions(sources);
        //        state.IntialiseDestinationTransitions(destinations);

        //        if (state.ParentId != null)
        //        {
        //            State? parent = null;
        //            var isCached = parentCache.TryGetValue(state.ParentId, out parent);
        //            if (!isCached)
        //            {
        //                parent = GetOrThrow(_states, state.ParentId, "Parent state");
        //            } else if(parent == null)
        //            {
        //                throw new InvalidOperationException("Parent should exist");
        //            }
        //            parent.AddChild(state);
        //            parentCache[state.ParentId] = parent;
        //        }

        //        if (state.Type == StateType.INITIAL)
        //        {
        //            this.intialState = state;
        //        }

        //        _states[state.Identifier] = state;
        //    }

        //    foreach (var parent in parentCache)
        //    {
        //        _childStates.Add(parent.Key, parent.Value.Children);
        //    }

        //}



    }
}
