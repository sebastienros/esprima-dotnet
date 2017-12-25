using System;

namespace Esprima
{
    public class ParserException : Exception
    {
        public int Column { get; set;}
        public string Description { get; set;}
        public int Index { get; set; }
        public int LineNumber { get; set;}

        public ParserException(string message) : base(message)
        {
        }
    }
}