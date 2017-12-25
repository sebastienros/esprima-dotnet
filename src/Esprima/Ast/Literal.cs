using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Esprima.Ast
{
    public class Literal : Node,
        Expression,
        PropertyKey
    {
        [JsonIgnore] public string StringValue { get; }
        [JsonIgnore] public double NumericValue { get; }
        [JsonIgnore] public bool BooleanValue { get; }
        [JsonIgnore] public Regex RegexValue { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public RegexValue Regex { get; }

        [JsonConverter(typeof(LiteralValueConverter))]
        public object Value { get; }

        public string Raw { get; }

        [JsonIgnore] public TokenType TokenType { get; }

        [JsonIgnore] public bool Integral { get; }

        [JsonIgnore] public bool Cached { get; }

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
            var s = Value as string;
            return s ?? Convert.ToString(Value);
        }
    }
}