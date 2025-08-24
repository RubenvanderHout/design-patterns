namespace IO.DTO;
public sealed record class FsmDto
{
    public List<StateDto> States { get; init; } = new();
    public List<TriggerDto> Triggers { get; init; } = new();
    public List<ActionDto> Actions { get; init; } = new();
    public List<TransitionDto> Transitions { get; init; } = new();
}