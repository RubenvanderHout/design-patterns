using System.Text.RegularExpressions;
using IO.DTO;

namespace IO;

/// <summary>
/// Parser for FSM files (STATE/TRIGGER/ACTION/TRANSITION; 
/// comments with '#'; each definition ends with ';').  
/// </summary>
public sealed class FsmFileParser : IParser
{
    // STATE <id> <parent> "<name>" : <type>;
    static readonly Regex StateRx = new(
        @"^STATE (?<id>[A-Za-z][A-Za-z0-9_]*) (?<parent>[A-Za-z][A-Za-z0-9_]*|_) ""(?<name>[^""]*)"" : (?<type>INITIAL|SIMPLE|COMPOUND|FINAL);$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // TRIGGER <id> "<description>";
    static readonly Regex TriggerRx = new(
        @"^TRIGGER (?<id>[A-Za-z][A-Za-z0-9_]*) ""(?<desc>[^""]*)"";$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // ACTION <owner> "<description>" : <type>;
    static readonly Regex ActionRx = new(
        @"^ACTION (?<owner>[A-Za-z][A-Za-z0-9_]*) ""(?<desc>[^""]*)"" : (?<type>ENTRY_ACTION|DO_ACTION|EXIT_ACTION|TRANSITION_ACTION);$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // TRANSITION <id> <src> -> <dst> [<trigger>] "<guard>";
    // Note: the trigger is optional; if present it has a trailing space before the quoted guard.
    static readonly Regex TransitionRx = new(
        @"^TRANSITION (?<id>[A-Za-z][A-Za-z0-9_]*) (?<src>[A-Za-z][A-Za-z0-9_]*) -> (?<dst>[A-Za-z][A-Za-z0-9_]*)(?: (?<trg>[A-Za-z][A-Za-z0-9_]*))? ""(?<guard>[^""]*)"";$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public FsmDto Parse(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            throw new ParseException("Input is empty.");

        var dto = new FsmDto();

        var normalized = Normalize(rawInput);
        var lines = normalized.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var lineNo = i + 1;
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (TryMatch(StateRx, line, out var m))
            {
                dto.States.Add(new StateDto
                {
                    Identifier = m!.Groups["id"].Value,
                    Parent = m.Groups["parent"].Value == "_" ? "_" : m.Groups["parent"].Value,
                    Name = m.Groups["name"].Value,
                    Type = Enum.Parse<StateType>(m.Groups["type"].Value, ignoreCase: false)
                });
                continue;
            }

            if (TryMatch(TriggerRx, line, out m))
            {
                dto.Triggers.Add(new TriggerDto
                {
                    Identifier = m!.Groups["id"].Value,
                    Description = m.Groups["desc"].Value
                });
                continue;
            }

            if (TryMatch(ActionRx, line, out m))
            {
                dto.Actions.Add(new ActionDto
                {
                    Identifier = m!.Groups["owner"].Value,
                    Description = m.Groups["desc"].Value,
                    Type = Enum.Parse<ActionType>(m.Groups["type"].Value, ignoreCase: false)
                });
                continue;
            }

            if (TryMatch(TransitionRx, line, out m))
            {
                dto.Transitions.Add(new TransitionDto
                {
                    Identifier = m!.Groups["id"].Value,
                    SourceStateIdentifier = m.Groups["src"].Value,
                    DestinationStateIdentifier = m.Groups["dst"].Value,
                    TriggerIdentifier = string.IsNullOrWhiteSpace(m.Groups["trg"].Value) ? null : m.Groups["trg"].Value,
                    GuardCondition = m.Groups["guard"].Value
                });
                continue;
            }

            throw new ParseException("Onbekende of ongeldige regel.", lineNo);
        }

        return dto;
    }

    static bool TryMatch(Regex rx, string line, out Match? m)
    {
        m = rx.Match(line);
        return m.Success;
    }

    static string Normalize(string input)
    {
        // strip comments (# ... end-of-line)
        var noComments = Regex.Replace(input, @"^\s*#.*$", string.Empty, RegexOptions.Multiline);
        var winFixed = noComments.Replace("\r\n", "\n").Replace('\r', '\n');
        return string.Join("\n", winFixed.Split('\n').Select(s => s.TrimEnd()));
    }
}
