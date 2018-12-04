using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Esprima.Ast
{
    public class Literal : Node,
        Expression
    {
        [JsonIgnore] public string StringValue => TokenType == TokenType.StringLiteral ? Value as string : null;
        [JsonIgnore] public readonly double NumericValue;
        [JsonIgnore] public bool BooleanValue => TokenType == TokenType.BooleanLiteral && NumericValue != 0;
        [JsonIgnore] public Regex RegexValue => TokenType == TokenType.RegularExpression ? (Regex) Value : null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public readonly RegexValue Regex;

        [JsonConverter(typeof(LiteralValueConverter))]
        public readonly object Value;

        public readonly string Raw;

        [JsonIgnore] public readonly TokenType TokenType;

        [JsonIgnore] public object CachedValue;

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