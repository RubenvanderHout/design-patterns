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

    public abstract record FsmNode(string Identifier)
    {
        public abstract void Accept(IValidtionRuleVistor visitor);
    }

    public sealed record State : FsmNode
    {
        public StateType Type { get; }

        public List<Action> Actions { get; private set; } = [];

        public List<State> Children { get; private set; } = [];
        public List<Transition> SourceTransitions { get; private set; } = [];
        public List<Transition> DestinationTransitions { get; private set; } = [];

        public State(
            string identifier,
            StateType type,
            List<Action> actions,
            List<State> childeren,
            List<Transition> sourceTransition,
            List<Transition> destinationTransaction
        ) : base(identifier)
        {
            Type = type;
            Actions = actions;
            Children = childeren;
            SourceTransitions = sourceTransition;
            DestinationTransitions = destinationTransaction;
        }

        public override void Accept(IValidtionRuleVistor visitor)
        {
            visitor.VisitState(this);
        }
    }

    public sealed record Trigger(string Identifier, string Description) : FsmNode(Identifier)
    {
        public override void Accept(IValidtionRuleVistor visitor)
        {
            visitor.VisitTrigger(this);
        }
    }

    public sealed record Action(string Identifier, string Description, ActionType Type) : FsmNode(Identifier)
    {
        public override void Accept(IValidtionRuleVistor visitor)
        {
            visitor.VisitAction(this);
        }
    }

    public sealed record Transition(
        string Identifier,
        State SourceState,
        State DestinatnionState,
        Trigger Trigger,
        Action? Action,
        string GuardCondition
    ) : FsmNode(Identifier)
    {
        public override void Accept(IValidtionRuleVistor visitor)
        {
            visitor.VisitTransition(this);
        }
    }
}
