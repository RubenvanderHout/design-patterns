using System.Text.RegularExpressions;
using IO.DTO;

namespace IO;

internal sealed class TransitionLineParser : BaseRegexLineParser
{
    // TRANSITION <id> <src> -> <dst> [<trigger>] "<guard>";
    public TransitionLineParser() : base(
        @"^TRANSITION (?<id>[A-Za-z][A-Za-z0-9_]*) (?<src>[A-Za-z][A-Za-z0-9_]*) -> (?<dst>[A-Za-z][A-Za-z0-9_]*)(?: (?<trg>[A-Za-z][A-Za-z0-9_]*))? ""(?<guard>[^""]*)"";$") {}

    protected override void ParseMatch(Match m, FsmDto dto)
    {
        dto.Transitions.Add(new TransitionDto
        {
            Identifier                = m.Groups["id"].Value,
            SourceStateIdentifier     = m.Groups["src"].Value,
            DestinationStateIdentifier= m.Groups["dst"].Value,
            TriggerIdentifier         = string.IsNullOrEmpty(m.Groups["trg"].Value) ? null : m.Groups["trg"].Value,
            GuardCondition            = m.Groups["guard"].Value
        });
    }
}
