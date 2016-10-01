namespace Esprima
{
    public interface IErrorHandler
    {
        bool Tolerant { get; set; }
        void RecordError(Error error);
        void Tolerate(Error error);
        void ThrowError(int index, int line, int column, string message);
        void TolerateError(int index, int line, int column, string message);
    }
}
