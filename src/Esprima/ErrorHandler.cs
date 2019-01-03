using System.Collections.Generic;

namespace Esprima
{
    public class ErrorHandler : IErrorHandler
    {
        public List<ParserException> Errors { get; }
        public bool Tolerant { get; set; }

        public string Source { get; set; }

        public ErrorHandler()
        {
            Errors = new List<ParserException>();
            Tolerant = false;
        }

        public void RecordError(ParserException error)
        {
            Errors.Add(error);
        }

        public void Tolerate(ParserException error)
        {
            if (Tolerant)
            {
                RecordError(error);
            }
            else
            {
                throw error;
            }
        }

        public ParserException CreateError(int index, int line, int col, string description)
        {
            var msg = $"Line {line}': {description}";
            var error = new ParserException(msg)
            {
                Index = index,
                Column = col,
                LineNumber = line,
                Description = description,
                Source = Source
            };
            return error;
        }

        public void TolerateError(int index, int line, int col, string description)
        {
            var error = this.CreateError(index, line, col, description);
            if (Tolerant)
            {
                this.RecordError(error);
            }
            else
            {
                throw error;
            }
        }
    }
}
