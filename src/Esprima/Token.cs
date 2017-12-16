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
        public TokenType Type;
        public string Literal;

        public int Start; // Range[0]
        public int End; // Range[1]
        public int LineNumber;
        public int LineStart;

        private Location _location;
        public Location Location => _location ?? (_location = new Location());

        public int Precedence;

        // For NumericLiteral
        public bool Octal;

        // For templates
        public bool Head;
        public bool Tail;
        public string RawTemplate;

        public bool BooleanValue;
        public double NumericValue;
        public object Value;
        public RegexValue RegexValue;

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
