using IO;
using IO.DTO;
using Xunit;

namespace IO.Tests;
public class ParserTests
{
    [Fact]
    public void Parses_Minimal_Spec_File()
    {
        var txt = """
        # states
        STATE initial _ "powered off" : INITIAL;
        STATE powered _ "Powered up" : COMPOUND;
        STATE off powered "Lamp is off" : SIMPLE;
        STATE on powered "Lamp is on" : SIMPLE;
        STATE final _ "powered off" : FINAL;

        # triggers
        TRIGGER power_on "turn power on";
        TRIGGER push_switch "Push switch";
        TRIGGER power_off "turn power off";

        # actions
        ACTION on "Turn lamp on" : ENTRY_ACTION;
        ACTION off "Turn lamp off" : EXIT_ACTION;
        ACTION t2 "reset off timer" : TRANSITION_ACTION;

        # transitions
        TRANSITION t1 initial -> off power_on "";
        TRANSITION t2 off -> on push_switch "time off > 10s";
        TRANSITION t3 on -> off push_switch "";
        TRANSITION t4 powered -> final power_off "";
        """;

        var parser = new FsmFileParser();
        var dto = parser.Parse(txt);

        Assert.Equal(5, dto.States.Count);
        Assert.Equal(3, dto.Triggers.Count);
        Assert.Equal(3, dto.Actions.Count);
        Assert.Equal(4, dto.Transitions.Count);
        Assert.Equal(StateType.COMPOUND, dto.States.Single(s => s.Identifier == "powered").Type);
        Assert.Equal("push_switch", dto.Transitions.Single(t => t.Identifier == "t2").TriggerIdentifier);
    }
}