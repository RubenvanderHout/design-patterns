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
    public sealed record RawState(
        string Id,
        string? ParentId,
        string Name,
        List<Action> actions,
        StateType type
    );

    public class FsmRepository
    {
        public Dictionary<string, State> RootStates { get; } = [];
        // StateID -> State
        public Dictionary<string, State> AllStates { get; } = [];
        // StateID -> List<State> children
        public Dictionary<string, List<State>> ChildStates { get; } = [];
        // StateId -> Source Transitions 
        public Dictionary<string, List<Transition>> SourceTransitions { get; } = [];
        // StateId -> Destination Transitions
        public Dictionary<string, List<Transition>> DestinationTransitions { get; } = [];

        public Dictionary<string, Trigger> Triggers { get; } = [];
        public Dictionary<string, List<Action>> Actions { get; } = [];

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

            foreach (var sItem in dto.States)
            {
                var stateType = MatchDtoToStateType(sItem.Type);

                var actions = Actions.GetValueOrDefault(sItem.Identifier, new List<Action>());

                var state = new State(sItem.Identifier, sItem.Name, sItem.Parent, stateType);

                state.Actions = actions;

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

                }
                else
                {
                    RootStates.Add(state.Identifier, state);
                }
                AllStates.Add(state.Identifier, state);
            }

            foreach (var state in AllStates.Values)
            {
                if (state.ParentId != null && AllStates.TryGetValue(state.ParentId, out var parentState))
                {
                    parentState.Children.Add(state);
                    state.Parent = parentState;
                }
            }

            foreach (var tItem in dto.Transitions)
            {
                var actions = Actions.GetValueOrDefault(tItem.Identifier, new List<Action>());
                var action = actions.Count > 0 ? actions[0] : null;


                Trigger? trigger = null;

                if (tItem.TriggerIdentifier != null)
                {
                    var hasTrigger = Triggers.TryGetValue(tItem.TriggerIdentifier, out trigger);

                    if (!hasTrigger)
                    {
                        throw new InvalidOperationException(
                            $"Error: Transition {tItem.Identifier} " +
                            $"references non existing trigger {tItem.TriggerIdentifier}"
                        );
                    }
                }

                if (!AllStates.TryGetValue(tItem.SourceStateIdentifier, out var sourceState) ||
               !AllStates.TryGetValue(tItem.DestinationStateIdentifier, out var destinationState))
                {
                    throw new InvalidOperationException(
                        $"Transition {tItem.Identifier} references non-existent states");
                }


                var transition = new Transition(
                    tItem.Identifier,
                    sourceState,
                    destinationState,
                    trigger,
                    action,
                    tItem.GuardCondition
                );

                sourceState.SourceTransitions.Add(transition);
                destinationState.DestinationTransitions.Add(transition);

                if (!SourceTransitions.TryGetValue(transition.SourceState.Identifier, out var sourceList))
                {
                    sourceList = [transition];
                    SourceTransitions.Add(transition.SourceState.Identifier, sourceList);
                }
                else
                {
                    sourceList.Add(transition);
                }

                if (!DestinationTransitions.TryGetValue(transition.DestinationState.Identifier, out var destList))
                {
                    destList = [transition];
                    DestinationTransitions.Add(transition.DestinationState.Identifier, destList);
                }
                else
                {
                    destList.Add(transition);
                }


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
