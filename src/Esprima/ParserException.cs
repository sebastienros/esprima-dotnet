using System;

namespace Esprima
{
    public class ParserException : Exception
    {
        public string Description { get; }
        public string SourceText  { get; }
        public int Index          { get; }
        public int LineNumber     { get; }
        public int Column         { get; }

        public ParserException() :
            this(null) {}

        public ParserException(string message) :
            this(message, null) {}

        public ParserException(string description,
            string sourceText, int index, int lineNumber, int column) :
            this(null, description, sourceText, index, lineNumber, column) {}

        public ParserException(string message, string description) :
            this(message, description, null, 0, 0, 0) {}

        public ParserException(string message, string description,
            string sourceText, int index, int lineNumber, int column) :
            base(message ?? FormatDefaultMessage(description, lineNumber))
        {
            Description = description;
            SourceText  = sourceText;
            Index       = index;
            LineNumber  = lineNumber;
            Column      = column;
        }

        static string FormatDefaultMessage(string description, int lineNumber)
            => description is string desc
             ? lineNumber > 0
             ? $"Line {lineNumber}: {desc}"
             : desc
             : null;
    }
}