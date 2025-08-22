using IO.DTO;
using Validation.ValidationRules;

namespace Validation
{
    public enum StateType
    {
        INITIAL,
        SIMPLE,
        COMPOUND,
        FINAL
    }

    public enum ActionType
    {
        ENTRY_ACTION,
        DO_ACTION,
        EXIT_ACTION,
        TRANSITION_ACTION
    }

    public abstract class FsmNode(string identifier)
    {
        public string Identifier { get; set; } = identifier;
    }

    public class State(
        string identifier,
        string? parentId,
        StateType type,
        List<Action> actions,
        List<Transition> sourceTransitions, 
        List<Transition> destinationTransitions
    ) : FsmNode(identifier)
    {
        public string? ParentId { get; set; } = parentId;
        public StateType Type { get; set; } = type;
        public List<Action> Actions { get; init; } = actions;
        public List<Transition> SourceTransitions { get; init; } = sourceTransitions;
        public List<Transition> DestinationTransitions { get; init; } = destinationTransitions;
        public List<State> Children { get; set; } = [];
    }

    public class Trigger(string identifier, string description) : FsmNode(identifier)
    {
        public string Description { get; set; } = description;
    }

    public class Action(string identifier, string description, ActionType type) : FsmNode(identifier)
    {
        public string Description { get; set; } = description;
        public ActionType Type { get; set; } = type;
    }

    public class Transition(
        string identifier,
        string sourceStateId,
        string destinationStateId,
        Trigger? trigger,
        Action action,
        string guardCondition
        ) : FsmNode(identifier)
    {
        public string SourceStateId { get; set; } = sourceStateId;
        public string DestinationStateId { get; set; } = destinationStateId;
        public Trigger? Trigger { get; set; } = trigger;
        public Action Action { get; set; } = action;
        public string GuardCondition { get; set; } = guardCondition;
    }
}
