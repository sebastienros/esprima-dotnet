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
        public static Token Empty = new Token();

        public TokenType Type;
        public string Literal;

        public int Start; // Range[0]
        public int End; // Range[1]
        public int LineNumber;
        public int LineStart;
        private Location _location;
        public Location Location
        {
            get
            {
                return _location ?? (_location = new Location());
            }
        }

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

        public Token()
        {
        }
    }
}
