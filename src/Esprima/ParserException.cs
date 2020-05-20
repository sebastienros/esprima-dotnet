using System;

namespace Esprima
{
    public class ParserException : Exception
    {
        public ParseError? Error  { get; }

        public string? Description => Error?.Description;
        public string? SourceText  => Error?.Source;
        public int Index          => Error?.Index ?? -1;
        public int LineNumber     => Error?.LineNumber ?? 0;
        public int Column         => Error?.Column ?? 0;

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
}
