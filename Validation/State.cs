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
    public enum ComponentType
    {
        STATE,
        TRANSACTION,
        TRIGGER,
        ACTION
    }

    public sealed record State(string Identifier, string? Parent, string Name, StateType Type);

    public sealed record Trigger(string Identifier, string Description);

    public sealed record ActionDef(string OwnerIdentifier, string Description, ActionType Type);

    public sealed record Transition(
        string Identifier,
        string Source,
        string Destination,
        string? TriggerIdentifier,
        string GuardCondition
    );
}
