using System;

namespace Esprima
{
    public class ParserException : Exception
    {
        public int Column;
        public string Description;
        public int Index;
        public int LineNumber;
        public string SourceText;

        public ParserException(string message) : base(message)
        {
        }
    }
}