namespace Esprima
{
    public interface IErrorHandler
    {
        bool Tolerant { get; set; }
        void RecordError(ParserException error);
        void Tolerate(ParserException error);
        ParserException CreateError(string? source, int index, int line, int column, string message);
        void TolerateError(string? source, int index, int line, int column, string message);
    }
}
