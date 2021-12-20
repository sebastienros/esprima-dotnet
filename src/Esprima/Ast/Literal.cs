using System;
using System.Numerics;
using System.Text.RegularExpressions;
using Esprima.Utils;

namespace Esprima.Ast
{
    public class Literal : Expression
    {
        public string? StringValue => TokenType == TokenType.StringLiteral ? Value as string : null;
        public readonly object NumericValue;
        public bool BooleanValue => TokenType == TokenType.BooleanLiteral && IsNotZero();
        public Regex? RegexValue => TokenType == TokenType.RegularExpression ? (Regex?) Value : null;

        public readonly RegexValue? Regex;
        public readonly object? Value;
        public readonly string Raw;
        public readonly TokenType TokenType;
        public NumericTokenType NumericTokenType;

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

        public Literal(NumericTokenType numTokenType, object value, string raw) : this(TokenType.NumericLiteral, value, raw)
        {
            if (numTokenType == NumericTokenType.None)
                throw new Exception("NumericTokenType is not valid");

            this.NumericTokenType = numTokenType;
            NumericValue = value;
        }

        public Literal(string raw) : this(TokenType.NilLiteral, null, raw)
        {
        }

        public Literal(string pattern, string flags, object? value, string raw) : this(TokenType.RegularExpression, value, raw)
        {
            // value is null if a Regex object couldn't be created out of the pattern or options
            Regex = new RegexValue(pattern, flags);
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitLiteral(this);
        }

        private bool IsNotZero()
        {
            if (NumericTokenType == NumericTokenType.Integer)
                return ((int)NumericValue) != 0;
            else
                throw new NotImplementedException("Not implemented");
        }
    }
}
