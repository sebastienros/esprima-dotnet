using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;
using Esprima.Utils;

namespace Esprima.Ast
{
    [DebuggerDisplay("{Raw,nq}")]
    public class Literal : Expression
    {
        public string? StringValue => TokenType == TokenType.StringLiteral ? Value as string : null;
        public readonly double NumericValue;
        public bool BooleanValue => TokenType == TokenType.BooleanLiteral && NumericValue != 0;
        public Regex? RegexValue => TokenType == TokenType.RegularExpression ? (Regex?) Value : null;
        public BigInteger? BigIntValue => TokenType == TokenType.BigIntLiteral ? (BigInteger?) Value : null;

        public readonly RegexValue? Regex;
        public readonly object? Value;
        public readonly string Raw;
        public readonly TokenType TokenType;

        internal Literal(TokenType tokenType, object? value, string raw) : base(Nodes.Literal)
        {
            TokenType = tokenType;
            Value = value;
            Raw = raw;
        }

        public Literal(string? value, string raw) : this(TokenType.StringLiteral, value, raw)
        {
        }

        public Literal(bool value, string raw) : this(TokenType.BooleanLiteral, value, raw)
        {
            NumericValue = value ? 1 : 0;
        }

        public Literal(double value, string raw) : this(TokenType.NumericLiteral, value, raw)
        {
            NumericValue = value;
        }

        public Literal(string raw) : this(TokenType.NullLiteral, null, raw)
        {
        }

        public Literal(string pattern, string flags, object? value, string raw) : this(TokenType.RegularExpression, value, raw)
        {
            // value is null if a Regex object couldn't be created out of the pattern or options
            Regex = new RegexValue(pattern, flags);
        }

        public sealed override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal sealed override void Accept(AstVisitor visitor)
        {
            visitor.VisitLiteral(this);
        }
    }
}
