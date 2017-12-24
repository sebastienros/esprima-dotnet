using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Esprima.Ast
{
    public class Literal : Node,
        Expression,
        PropertyKey
    {
        [JsonIgnore] public readonly string StringValue;
        [JsonIgnore] public readonly double NumericValue;
        [JsonIgnore] public readonly bool BooleanValue;
        [JsonIgnore] public readonly Regex RegexValue;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public readonly RegexValue Regex;

        [JsonConverter(typeof(LiteralValueConverter))]
        public readonly object Value;

        public readonly string Raw;

        [JsonIgnore] public readonly TokenType TokenType;

        [JsonIgnore] public readonly bool Integral;

        [JsonIgnore] public readonly bool Cached;

        //[JsonIgnore]
        //public JsValue CachedValue;

        public Literal(string value, string raw)
        {
            Type = Nodes.Literal;
            Value = StringValue = value;
            TokenType = TokenType.StringLiteral;
            Raw = raw;
        }

        public Literal(bool value, string raw)
        {
            Type = Nodes.Literal;
            Value = BooleanValue = value;
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
            RegexValue = (Regex) value;
            Regex = new RegexValue(pattern, flags);
            TokenType = TokenType.RegularExpression;
            Raw = raw;
        }

        string PropertyKey.GetKey()
        {
            return Convert.ToString(Value);
        }
    }
}