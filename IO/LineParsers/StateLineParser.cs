using System.Text.RegularExpressions;
using IO.DTO;

namespace IO;

internal sealed class StateLineParser : BaseRegexLineParser
{
    // STATE <id> <parent> "<name>" : <type>;
    public StateLineParser() : base(
        @"^STATE (?<id>[A-Za-z][A-Za-z0-9_]*) (?<parent>[A-Za-z][A-Za-z0-9_]*|_) ""(?<name>[^""]*)"" : (?<type>INITIAL|SIMPLE|COMPOUND|FINAL);$") {}

    protected override void ParseMatch(Match m, FsmDto dto)
    {
        dto.States.Add(new StateDto
        {
            Identifier = m.Groups["id"].Value,
            Parent     = m.Groups["parent"].Value == "_" ? null : m.Groups["parent"].Value,
            Name       = m.Groups["name"].Value,
            Type       = Enum.Parse<StateType>(m.Groups["type"].Value, ignoreCase: false)
        });
    }
}
