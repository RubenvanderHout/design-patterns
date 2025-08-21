using System.Text.RegularExpressions;
using IO.DTO;

namespace IO;

/// <summary>
/// Parser for FSM files using strategy parsers per line type.
/// Strict spacing; one definition per line; comments start with '#'.
/// </summary>
public sealed class FsmFileParser : IParser
{
    private readonly IReadOnlyList<ILineParser> _parsers;

    public FsmFileParser()
        : this(new ILineParser[]
        {
            new StateLineParser(),
            new TriggerLineParser(),
            new ActionLineParser(),
            new TransitionLineParser()
        })
    { }

    public FsmFileParser(IEnumerable<ILineParser> parsers)
    {
        _parsers = parsers?.ToList() ?? throw new ArgumentNullException(nameof(parsers));
        if (_parsers.Count == 0) throw new ArgumentException("At least one line parser is required.", nameof(parsers));
    }

    public FsmDto Parse(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            throw new ParseException("Input is empty.");

        var dto = new FsmDto();
        var lines = Normalize(rawInput).Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var lineNo = i + 1;
            var line = lines[i];
            if (line.Length == 0) continue; // skip blank lines

            var handled = false;
            foreach (var p in _parsers)
            {
                if (p.TryParse(line, dto, lineNo))
                {
                    handled = true;
                    break;
                }
            }

            if (!handled)
                throw new ParseException("Unrecognized or invalid line.", lineNo);
        }

        return dto;
    }

    static string Normalize(string input)
    {
        // Remove full-line comments, preserve other whitespace
        var noComments = Regex.Replace(input, @"^\s*#.*$", string.Empty, RegexOptions.Multiline);
        var lf = noComments.Replace("\r\n", "\n").Replace('\r', '\n');
        // Remove trailing spaces only
        return string.Join("\n", lf.Split('\n').Select(s => s.TrimEnd()));
    }
}
