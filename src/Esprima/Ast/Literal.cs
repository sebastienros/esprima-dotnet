using System.Text.RegularExpressions;

namespace Esprima.Ast
{
    public class Literal : Node,
        Expression
    {
        public string StringValue => TokenType == TokenType.StringLiteral ? Value as string : null;
        public readonly double NumericValue;
        public bool BooleanValue => TokenType == TokenType.BooleanLiteral && NumericValue != 0;
        public Regex RegexValue => TokenType == TokenType.RegularExpression ? (Regex) Value : null;

        public readonly RegexValue Regex;
        public readonly object Value;
        public readonly string Raw;
        public readonly TokenType TokenType;
        public object CachedValue;

        public Literal(string value, string raw)
        {
            Type = Nodes.Literal;
            Value = value;
            TokenType = TokenType.StringLiteral;
            Raw = raw;
        }

        public Literal(bool value, string raw)
        {
            Type = Nodes.Literal;
            Value = value;
            NumericValue = value ? 1 : 0;
            TokenType = TokenType.BooleanLiteral;
            Raw = raw;
        }

        public Literal(double value, string raw)
        {
            Type = Nodes.Literal;
            Value = NumericValue = value;
            TokenType = TokenType.NumericLiteral;
            Raw = raw;
        }

        public Literal(string raw)
        {
            Type = Nodes.Literal;
            Value = null;
            TokenType = TokenType.NullLiteral;
            Raw = raw;
        }

        public Literal(string pattern, string flags, object value, string raw)
        {
            Type = Nodes.Literal;
            // value is null if a Regex object couldn't be created out of the pattern or options
            Value = value;
            Regex = new RegexValue(pattern, flags);
            TokenType = TokenType.RegularExpression;
            Raw = raw;
        }
    }
}