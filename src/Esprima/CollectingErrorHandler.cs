using System.Collections.Generic;

namespace Esprima
{
    /// <summary>
    /// Error handler that collects errors that have been seen during the parsing.
    /// </summary>
    /// <remarks>
    /// If you reuse this instance memory usage can grow during process lifetime when errors
    /// are gathered.
    /// </remarks>
    public sealed class CollectingErrorHandler : ErrorHandler
    {
        private readonly List<ParserException> _errors = new List<ParserException>();

        public IReadOnlyCollection<ParserException> Errors => _errors;

        public override void RecordError(ParserException error)
        {
            _errors.Add(error);
        }
    }
}
