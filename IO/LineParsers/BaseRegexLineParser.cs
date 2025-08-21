using System.Text.RegularExpressions;

namespace IO;

internal abstract class BaseRegexLineParser : ILineParser
{
    protected readonly Regex Rx;
    protected BaseRegexLineParser(string pattern)
    {
        Rx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }

    public bool TryParse(string line, IO.DTO.FsmDto dto, int lineNumber)
    {
        var m = Rx.Match(line);
        if (!m.Success) return false;
        try
        {
            ParseMatch(m, dto);
            return true;
        }
        catch (ParseException) { throw; }
        catch (Exception ex)   { throw new ParseException(ex.Message, lineNumber, ex); }
    }

    protected abstract void ParseMatch(Match m, IO.DTO.FsmDto dto);
}
