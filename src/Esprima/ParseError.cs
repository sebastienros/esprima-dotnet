namespace Esprima;

public sealed class ParseError
{
    public string Description { get; }

    public string? Source { get; }

    public bool IsIndexDefined => Index >= 0;

    /// <summary>
    /// Zero-based index within the parsed code string. (Can be negative if location information is available.)
    /// </summary>
    public int Index { get; }

    public bool IsPositionDefined => Position.Line > 0;

    public Position Position { get; }

    /// <summary>
    /// One-based line number. (Can be zero if location information is not available.)
    /// </summary>
    public int LineNumber => Position.Line;

    /// <summary>
    /// One-based column index.
    /// </summary>
    public int Column => Position.Column + 1;

    public ParseError(string description, string? source = null, int index = -1, Position position = default)
    {
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Source = source;
        Index = index;
        Position = position;
    }

    public override string ToString()
    {
        return LineNumber > 0 ? $"Line {LineNumber}: {Description}" : Description;
    }

    public ParserException ToException()
    {
        return new ParserException(this);
    }
}
