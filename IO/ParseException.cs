namespace IO;

public sealed class ParseException : Exception
{
    public int? LineNumber { get; }
    public ParseException(string message, int? lineNumber = null, Exception? inner = null)
        : base(lineNumber is null ? message : $"Line {lineNumber}: {message}", inner)
        => LineNumber = lineNumber;
}