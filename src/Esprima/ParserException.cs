using System;

namespace Esprima
{
    public class ParserException : Exception
    {
        public int Column;
        public string Description;
        public int Index;
        public int LineNumber;

        public ParserException(string message) : base(message)
        {
        }
    }
}