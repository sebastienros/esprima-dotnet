namespace Esprima;

/// <summary>
/// Error handler that collects errors that have been seen during the parsing.
/// </summary>
public sealed class CollectingErrorHandler : ErrorHandler
{
    private readonly List<ParserException> _errors = new();

    public IReadOnlyCollection<ParserException> Errors => _errors;

    protected internal override void Reset()
    {
        _errors.Clear();
        if (_errors.Capacity > 16)
        {
            _errors.Capacity = 16;
        }
    }

    protected override void RecordError(ParserException error)
    {
        _errors.Add(error);
    }
}
