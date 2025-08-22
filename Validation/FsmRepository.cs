using IO.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Validation
{
    //Should be removed when the ui is changed
    public sealed record RawState(
        string Id,
        string? ParentId,
        string Name,
        List<Action> actions,
        StateType type
    );

    public class FsmRepository
    {
        public List<State> RootStates { get; } = [];
        // StateID -> State
        public Dictionary<string, State> AllStates { get; } = [];
        // StateID -> List<State> children
        public Dictionary<string, List<State>> ChildStates { get; } = [];
        // StateId -> Source Transitions 
        public Dictionary<string, List<Transition>> SourceTransitions { get; } = [];
        // StateId -> Destination Transitions
        public Dictionary<string, List<Transition>> DestinationTransitions { get; } = [];

        // Triggers and Actions both have a 1 on 1 relationship 
        // So I chose to have them already inserted into the objects here
        // for performance and ease of use
        private Dictionary<string, Trigger> Triggers { get; } = [];
        private Dictionary<string, List<Action>> Actions { get; } = [];

        public FsmRepository(FsmDto dto)
        {
            BuildIndexes(dto);
        }

        private void BuildIndexes(in FsmDto dto)
        {
            foreach (var triItem in dto.Triggers)
            {
                var trigger = new Trigger(triItem.Identifier, triItem.Description);
                Triggers.Add(trigger.Identifier, trigger);
            }

            foreach (var aItem in dto.Actions)
            {
                var actionType = MatchDtoToActionType(aItem.Type);
                var action = new Action(aItem.Identifier, aItem.Description, actionType);

                if (Actions.TryGetValue(aItem.Identifier, out var list))
                {
                    list.Add(action);
                }
                else
                {
                    var listNew = new List<Action>
                    {
                        action
                    };
                    Actions.Add(action.Identifier, listNew);
                }
            }

            foreach (var tItem in dto.Transitions)
            {
                var actions = Actions.GetValueOrDefault(tItem.Identifier, new List<Action>());

                Trigger? trigger = null;

                if (tItem.TriggerIdentifier != null)
                {
                    var hasTrigger = Triggers.TryGetValue(tItem.TriggerIdentifier, out trigger);

                    if (!hasTrigger)
                    {
                        // This is the only validation that happens in the repository.
                        // Most others should happen in the rule parser but because this is a reference to something
                        // That doesn't exist this is a valid exception the user really did something bad
                        throw new InvalidOperationException(
                            $"Error: Transition {tItem.Identifier} " +
                            $"references non existing trigger {tItem.TriggerIdentifier}"
                        );
                    }
                }

                var transition = new Transition(
                    tItem.Identifier,
                    tItem.SourceStateIdentifier,
                    tItem.DestinationStateIdentifier,
                    trigger,
                    actions[0], // Only one action per transition
                    tItem.GuardCondition
                );

                if (!SourceTransitions.TryGetValue(transition.SourceStateId, out var sourceList))
                {
                    sourceList = [transition];
                    SourceTransitions.Add(transition.SourceStateId, sourceList);
                }
                else
                {
                    sourceList.Add(transition);
                }

                if (!DestinationTransitions.TryGetValue(transition.DestinationStateId, out var destList))
                {
                    destList = [transition];
                    DestinationTransitions.Add(transition.DestinationStateId, destList);
                }
                else
                {
                    destList.Add(transition);
                }

            }

            foreach (var sItem in dto.States)
            {
                var stateType = MatchDtoToStateType(sItem.Type);

                var actions = Actions.GetValueOrDefault(sItem.Identifier, new List<Action>());
                var sources = SourceTransitions.GetValueOrDefault(sItem.Identifier, new List<Transition>());
                var destinations = SourceTransitions.GetValueOrDefault(sItem.Identifier, new List<Transition>());

                var state = new State(sItem.Identifier, sItem.Parent, stateType, actions, sources, destinations);

                if (state.ParentId != null)
                {
                    if (!ChildStates.TryGetValue(state.ParentId, out var childList))
                    {
                        childList = [state];
                        ChildStates.Add(state.ParentId, childList);
                    }
                    else
                    {
                        childList.Add(state);
                    }

                } else
                {
                    RootStates.Add(state);
                }
                AllStates.Add(state.Identifier, state);
            }

            var parents = RootStates;

            foreach (var parent in parents)
            {
                parent.Children = ChildStates.GetValueOrDefault(parent.Identifier, []);
            }

        }

        public static StateType MatchDtoToStateType(IO.DTO.StateType stateType)
        {
            var vst = stateType switch
            {
                IO.DTO.StateType.INITIAL => StateType.INITIAL,
                IO.DTO.StateType.SIMPLE => StateType.SIMPLE,
                IO.DTO.StateType.COMPOUND => StateType.COMPOUND,
                IO.DTO.StateType.FINAL => StateType.FINAL,
                _ => throw new NotImplementedException("Dev error: handle the other cases of the DTO"),
            };
            return vst;
        }

        public static ActionType MatchDtoToActionType(IO.DTO.ActionType actionType)
        {
            var vst = actionType switch
            {
                IO.DTO.ActionType.ENTRY_ACTION => ActionType.ENTRY_ACTION,
                IO.DTO.ActionType.DO_ACTION => ActionType.DO_ACTION,
                IO.DTO.ActionType.EXIT_ACTION => ActionType.EXIT_ACTION,
                IO.DTO.ActionType.TRANSITION_ACTION => ActionType.TRANSITION_ACTION,
                _ => throw new NotImplementedException("Dev error: handle the other cases of the DTO"),
            };
            return vst;
        }

        
    }
}
