namespace Esprima;

public sealed class ParserException : Exception
{
    public ParseError? Error { get; }

    public string? Description => Error?.Description;

    public string? SourceLocation => Error?.Source;

    /// <summary>
    /// Zero-based index within the parsed code string. (Can be negative if location information is available.)
    /// </summary>
    public int Index => Error?.Index ?? -1;

    /// <summary>
    /// One-based line number. (Can be zero if location information is not available.)
    /// </summary>
    public int LineNumber => Error?.LineNumber ?? 0;

    /// <summary>
    /// One-based column index.
    /// </summary>
    public int Column => Error?.Column ?? 1;

    public ParserException() : this(null, null, null)
    {
    }

    public ParserException(ParseError error) : this(null, null, error)
    {
    }

    public ParserException(string? message, Exception? innerException = null, ParseError? error = null)
        : base(message ?? error?.ToString(), innerException)
    {
        Error = error;
    }
}
