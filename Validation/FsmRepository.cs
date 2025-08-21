using IO.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Validation
{
    public sealed record RawState(
        string Id,
        string? ParentId,
        string Name,
        StateType type
    );

    public sealed record RawTransition(
        string Id, 
        string SourceStateId, 
        string DestinationStateId, 
        string? TriggerId,
        string GuardCondition
    );

    public class FsmRepository
    {
        // TriggerId -> Trigger
        public Dictionary<string, Trigger> Triggers { get; } = [];
        // TransitionId or StateId -> Action
        public Dictionary<string, List<Action>> Actions { get; } = [];
      
        // StateId -> Source Transitions 
        public readonly Dictionary<string, List<RawTransition>> SourceTransitions = [];
        // StateId -> Destination Transitions
        public readonly Dictionary<string, List<RawTransition>> DestinationTransitions = [];
        // StateID -> State
        public readonly Dictionary<string, RawState> RawStates = [];
        // StatID -> List<State> children
        public readonly Dictionary <string, List<RawState>> RawChildren = [];

        public RawState? RootState { get => rootState; }
        private RawState? rootState = null;

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
                var state = new RawState(sItem.Identifier, sItem.Parent, sItem.Name, stateType);

                if(state.type == StateType.INITIAL)
                {
                    rootState = state;
                }

                if (state.ParentId != null)
                {
                    if (!RawChildren.TryGetValue(state.ParentId, out var childList))
                    {
                        childList = [state];
                        RawChildren.Add(state.ParentId, childList);
                    }
                    else
                    {
                        childList.Add(state);
                    }

                }
                else
                {
                    RawStates.Add(state.Id, state);
                }

            }

            foreach (var tItem in dto.Transitions)
            {
                var transition = new RawTransition(
                    tItem.Identifier, 
                    tItem.SourceStateIdentifier, 
                    tItem.DestinationStateIdentifier, 
                    tItem.TriggerIdentifier,
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
