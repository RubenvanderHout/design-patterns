using IO.DTO;

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

    public abstract record FsmNode(string Identifier);

    public sealed record State : FsmNode
    {
        public State? Parent { get; private set; }
        public List<State> Children { get; } = [];
        public List<Action> Actions { get; } = [];
        // Allow these to be later passed in. Because i can't create such a Transactions right away.
        private List<Transition> LeavingTransitions = [];
        private List<Transition> EnteringTransitions = [];

        private bool LeavingTransitionsIntialised = false;
        private bool EnteringTransitionsIntialised = false;

        public StateType Type { get; }

        public string? ParentId { get; } 

        public State(
            string identifier,
            StateType type,
            string? parent
        ) : base(identifier)
        {
            ParentId = parent;
            Type = type;

        }

        public void AddParent(State state)
        {
            if (ParentId == null)
            {
                throw new InvalidOperationException(
                    "State has no specified parent illegal operation"
                );
            }

            if (state.Identifier == ParentId)
            {
                Parent = state;
            }
            else
            {
                throw new InvalidOperationException(
                    $"State given isn't the correct one expected: ${ParentId} got ${state.Identifier}"
                );
            }
           
        }

        public void AddChild(State child)
        {
            Children.Add(child);
        }

        public void AddAction(Action action)
        {
            Actions.Add(action);
        }

        public void LeavingTransitions(List<Transition> LeavingTransitions)
        {
            if (LeavingTransitionsIsSet)
            {

            }

        }

        public void EnteringTransitions(List<Transition> EnteringTransitions)
        {


        }
    }

    public sealed record Trigger(string Identifier, string Description) : FsmNode(Identifier);

    public sealed record Action(string Identifier, string Description, ActionType Type) : FsmNode(Identifier);

    public sealed record Transition(
        string Identifier,
        State SourceState,
        State DestinatnionState,
        Trigger Trigger,
        Action? Action,
        string GuardCondition
    ) : FsmNode(Identifier);

}
