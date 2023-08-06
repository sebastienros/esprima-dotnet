namespace Esprima;

/// <summary>
/// Default error handling logic for Esprima.
/// </summary>
public class ErrorHandler
{
    public static readonly ErrorHandler Default = new();

    protected internal virtual void Reset() { }

    protected virtual void RecordError(ParseError error)
    {
    }

    internal void Tolerate(ParseError error, bool tolerant)
    {
        if (tolerant)
        {
            RecordError(error);
        }
        else
        {
            throw error.ToException();
        }
    }

    protected internal virtual ParseError CreateError(string? source, int index, int line, int col, string description)
    {
        return new ParseError(description, source, index, new Position(line, col));
    }

    internal ParseError TolerateError(string? source, int index, int line, int col, string description, bool tolerant)
    {
        var error = CreateError(source, index, line, col, description);
        Tolerate(error, tolerant);
        return error;
    }
}
