namespace Esprima;

/// <summary>
/// Default error handling logic for Esprima.
/// </summary>
public class ErrorHandler : IErrorHandler
{
    public virtual void RecordError(ParserException error)
    {
    }

    public void Tolerate(ParserException error, bool tolerant)
    {
        if (tolerant)
        {
            RecordError(error);
        }
        else
        {
            throw error;
        }
    }

    public ParserException CreateError(string? source, int index, int line, int col, string description)
    {
        return new(new ParseError(description, source, index, new Position(line, col)));
    }

    public void TolerateError(string? source, int index, int line, int col, string description, bool tolerant)
    {
        var error = CreateError(source, index, line, col, description);
        Tolerate(error, tolerant);
    }
}
