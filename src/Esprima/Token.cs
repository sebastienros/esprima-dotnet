using Esprima.Ast;

namespace Esprima
{
    public enum TokenType
    {
        BooleanLiteral,
        EOF,
        Identifier,
        Keyword,
        NullLiteral,
        NumericLiteral,
        Punctuator,
        StringLiteral,
        RegularExpression,
        Template
    };

    public class Token
    {
        public TokenType Type { get; internal set; }
        public string Literal { get; internal set; }

        public int Start { get; internal set; } // Range[0]
        public int End { get; internal set; } // Range[1]
        public int LineNumber { get; internal set; }
        public int LineStart { get; internal set; }

        private Location _location;
        public Location Location => _location ?? (_location = new Location());

        public int Precedence { get; internal set; }

        // For NumericLiteral
        public bool Octal { get; internal set; }

        // For templates
        public bool Head { get; internal set; }
        public bool Tail { get; internal set; }
        public string RawTemplate { get; internal set; }

        public bool BooleanValue { get; internal set; }
        public double NumericValue { get; internal set; }
        public object Value { get; internal set; }
        public RegexValue RegexValue { get; internal set;}

        public void Clear()
        {
            Type = TokenType.BooleanLiteral;
            Literal = null;
            Start = 0;
            End = 0;
            LineNumber = 0;
            LineStart = 0;
            _location = null;
            Precedence = 0;
            Octal = false;
            Head = false;
            Tail = false;
            RawTemplate = null;
            BooleanValue = false;
            NumericValue = 0;
            Value = null;
            RegexValue = null;
        }
    }
}
