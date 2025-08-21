using System.Text.RegularExpressions;
using IO.DTO;

namespace IO;

internal sealed class ActionLineParser : BaseRegexLineParser
{
    // ACTION <owner> "<description>" : <type>;
    public ActionLineParser() : base(
        @"^ACTION (?<owner>[A-Za-z][A-Za-z0-9_]*) ""(?<desc>[^""]*)"" : (?<type>ENTRY_ACTION|DO_ACTION|EXIT_ACTION|TRANSITION_ACTION);$") {}

    protected override void ParseMatch(Match m, FsmDto dto)
    {
        dto.Actions.Add(new ActionDto
        {
            Identifier  = m.Groups["owner"].Value,
            Description = m.Groups["desc"].Value,
            Type        = Enum.Parse<ActionType>(m.Groups["type"].Value, ignoreCase: false)
        });
    }
}
