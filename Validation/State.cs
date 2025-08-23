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

    public interface IFsmElement
    {
        void Accept(IVisitor visitor);
    }

    public abstract class FsmNode : IFsmElement
    {
        public string Identifier { get; set; }

        protected FsmNode(string identifier)
        {
            Identifier = identifier;
        }

        public abstract void Accept(IVisitor visitor);
    }

    public class State(
        string identifier,
        string name, 
        string? parentId,
        StateType type
    ) : FsmNode(identifier) 
    {
        public string? ParentId { get; set; } = parentId;
        public string Name { get; set; } = name;
        public StateType Type { get; set; } = type;
        public List<Action> Actions { get; set; } = [];
        public List<Transition> SourceTransitions { get; set; } = [];
        public List<Transition> DestinationTransitions { get; set; } = [];
        public List<State> Children { get; set; } = [];
        public State? Parent { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);

            foreach (var child in Children)
            {
                visitor.Visit(child);
            }

            foreach (var transition in SourceTransitions)
            {
                visitor.Visit(transition);
            }
        }
    }

    public class Trigger(string identifier, string description) : FsmNode(identifier)
    {
        public string Description { get; set; } = description;

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Action(string identifier, string description, ActionType type) : FsmNode(identifier)
    {
        public string Description { get; set; } = description;
        public ActionType Type { get; set; } = type;

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Transition(
        string identifier,
        State sourceState,
        State destinationState,
        Trigger? trigger,
        Action? action,
        string guardCondition
        ) : FsmNode(identifier)
    {
        public State SourceState { get; set; } = sourceState;
        public State DestinationState { get; set; } = destinationState;
        public Trigger? Trigger { get; set; } = trigger;
        public Action? Action { get; set; } = action;
        public string GuardCondition { get; set; } = guardCondition;

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
            
            if(Action != null)
            {
                visitor.Visit(Action);
            }

            if (Trigger != null)
            {
                visitor.Visit(Trigger);
            }
        }
    }
}
