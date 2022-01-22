using System.Numerics;
using Esprima.Ast;

namespace Esprima
{
    public enum TokenType
    {
        BooleanLiteral,
        EOF,
        Identifier,
        Keyword,
        NilLiteral,
        NumericLiteral,
        Punctuator,
        StringLiteral,
        RegularExpression,
        Template,
        SymbolLiteral,
    };

    public enum NumericTokenType
    {
        None,
        Integer,
        UnsignedInteger,
        Long,
        UnsignedLong,
        Float,
        Double,
    }

    public class Token
    {
        public TokenType Type;
        public string? Literal;

        public int Start; // Range[0]
        public int End; // Range[1]
        public int LineNumber;
        public int LineStart;

        public Location Location;

        // For NumericLiteral
        public bool Octal;
        public char? NotEscapeSequenceHead;

        // For templates
        public bool Head;
        public bool Tail;
        public string? RawTemplate;

        public bool BooleanValue;
        public object NumericValue;
        public object? Value;
        public RegexValue? RegexValue;

        public NumericTokenType NumericTokenType;

        public void Clear()
        {
            Type = TokenType.BooleanLiteral;
            Literal = null;
            Start = 0;
            End = 0;
            LineNumber = 0;
            LineStart = 0;
            Location = default;
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
