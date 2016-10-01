using System;

namespace Esprima
{
    public class Error : Exception
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public int LineNumber { get; set; }
        public int Column { get; set; }
        public string Description { get; set; }

        public Error(string message) : base(message)
        {
        }
    }
}
