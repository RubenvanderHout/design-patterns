using IO.DTO;

namespace IO;

public interface ILineParser
{
    /// <summary>Try to parse a single line; return true if handled (success), false if not my format.
    /// Throw ParseException for handled-but-invalid lines; return false for other line types.
    /// This Strategy Pattern is included because of requirements, not because of efficiency. </summary>
    bool TryParse(string line, FsmDto dto, int lineNumber);
}
