namespace Esprima;

public sealed class ParserException : Exception
{
    public ParseError? Error { get; }

    public string? Description => Error?.Description;
    public string? SourceLocation => Error?.Source;
    /// <summary>
    /// Zero-based index within <see cref="SourceLocation"/>. (Can be negative if code is not available.)
    /// </summary>
    public int Index => Error?.Index ?? -1;
    /// <summary>
    /// One-based line number. (Can be zero if code is not available.)
    /// </summary>
    public int LineNumber => Error?.LineNumber ?? 0;
    /// <summary>
    /// One-based column index.
    /// </summary>
    public int Column => Error?.Column ?? 1;

    public ParserException() : this(null, null, null)
    {
    }

    public ParserException(string? message, Exception innerException) : this(message, null, innerException)
    {
    }

    public ParserException(ParseError error) : this(null, error)
    {
    }

    public ParserException(string? message, ParseError error) : this(message, error, null)
    {
    }

    public ParserException(string? message, ParseError? error = null, Exception? innerException = null)
        : base(message ?? error?.ToString(), innerException)
    {
        Error = error;
    }
}
