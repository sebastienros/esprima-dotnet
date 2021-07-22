namespace Esprima;

public interface IErrorHandler
{
    void RecordError(ParserException error);
    void Tolerate(ParserException error, bool tolerant);
    ParserException CreateError(string? source, int index, int line, int column, string message);
    void TolerateError(string? source, int index, int line, int column, string message, bool tolerant);
}
