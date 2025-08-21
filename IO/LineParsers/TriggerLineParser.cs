using System.Text.RegularExpressions;
using IO.DTO;

namespace IO;

internal sealed class TriggerLineParser : BaseRegexLineParser
{
    // TRIGGER <id> "<description>";
    public TriggerLineParser() : base(
        @"^TRIGGER (?<id>[A-Za-z][A-Za-z0-9_]*) ""(?<desc>[^""]*)"";$") {}

    protected override void ParseMatch(Match m, FsmDto dto)
    {
        dto.Triggers.Add(new TriggerDto
        {
            Identifier  = m.Groups["id"].Value,
            Description = m.Groups["desc"].Value
        });
    }
}
