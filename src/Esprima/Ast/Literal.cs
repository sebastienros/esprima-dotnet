using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Esprima.Ast
{
    public class Literal : Node,
        Expression,
        PropertyKey
    {
        // common literals that can be cached
        private static readonly Literal StringEmpty = new Literal("", "", cached: true);
        private static readonly Literal StringDoubleQuoteEmpty = new Literal("", "\"\"", cached: true);
        private static readonly Literal StringSingleQuoteEmpty = new Literal("", "''", cached: true);
        private static readonly Literal StringDoubleQuoteSpace = new Literal(" ", "\" \"", cached: true);
        private static readonly Literal StringSingleQuoteSpace = new Literal(" ", "' '", cached: true);

        private static readonly Literal NumericZero = new Literal(0, "0", cached: true);
        private static readonly Literal NumericOne = new Literal(1, "1", cached: true);
        private static readonly Literal NumericTwo = new Literal(2, "2", cached: true);
        private static readonly Literal NumericThree = new Literal(3, "3", cached: true);
        private static readonly Literal NumericFour = new Literal(4, "4", cached: true);
        private static readonly Literal NumericFive = new Literal(5, "5", cached: true);
        private static readonly Literal NumericSix = new Literal(6, "6", cached: true);
        private static readonly Literal NumericSeven = new Literal(7, "7", cached: true);
        private static readonly Literal NumericEight = new Literal(8, "8", cached: true);
        private static readonly Literal NumericNine = new Literal(9, "9", cached: true);
        private static readonly Literal NumericTen = new Literal(10, "10", cached: true);
        private static readonly Literal NumericEleven = new Literal(11, "11", cached: true);
        private static readonly Literal NumericHundred = new Literal(100, "100", cached: true);

        public static readonly Literal Null = new Literal("null");

        public static readonly Literal BooleanFalse = new Literal(false);
        public static readonly Literal BooleanTrue = new Literal(true);

        [JsonIgnore] public string StringValue => Value as string;
        [JsonIgnore] public readonly double NumericValue;
        [JsonIgnore] public bool BooleanValue => NumericValue != 0;
        [JsonIgnore] public readonly Regex RegexValue;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public readonly RegexValue Regex;

        [JsonConverter(typeof(LiteralValueConverter))]
        public readonly object Value;

        public readonly string Raw;

        [JsonIgnore] public readonly TokenType TokenType;

        [JsonIgnore] public readonly bool Cached;

        public Literal(string value, string raw) : this(value, raw, false)
        {
        }

        private Literal(string value, string raw, bool cached)
        {
            Type = Nodes.Literal;
            Value = value;
            TokenType = TokenType.StringLiteral;
            Raw = raw;
            Cached = cached;
        }

        private Literal(bool value)
        {
            Type = Nodes.Literal;
            Value = value;
            NumericValue = value ? 1.0 : 0.0;
            TokenType = TokenType.BooleanLiteral;
            Raw = value ? "true" : "false";
            Cached = true;
        }

        public Literal(double value, string raw) : this(value, raw, false)
        {
        }

        private Literal(double value, string raw, bool cached)
        {
            Type = Nodes.Literal;
            Value = NumericValue = value;
            TokenType = TokenType.NumericLiteral;
            Raw = raw;
            Cached = cached;
        }

        private Literal(string raw)
        {
            Type = Nodes.Literal;
            Value = null;
            TokenType = TokenType.NullLiteral;
            Raw = raw;
            Cached = true;
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

        internal static Literal CreateStringLiteral(string value, string raw)
        {
            if (value == "" && raw == "")
            {
                return StringEmpty;
            }
            if (value == "" && raw == "\"\"")
            {
                return StringDoubleQuoteEmpty;
            }
            if (value == "" && raw == "''")
            {
                return StringSingleQuoteEmpty;
            }
            if (value == " " && raw == "\" \"")
            {
                return StringDoubleQuoteSpace;
            }
            if (value == " " && raw == "' '")
            {
                return StringSingleQuoteSpace;
            }

            return new Literal(value, raw);
        }

        internal static Literal CreateNumericLiteral(double value, string raw)
        {
            if (value == 0 && raw == "0")
            {
                return NumericZero;
            }
            if (value == 1 && raw == "1")
            {
                return NumericOne;
            }
            if (value == 2 && raw == "2")
            {
                return NumericTwo;
            }
            if (value == 3 && raw == "3")
            {
                return NumericThree;
            }
            if (value == 4 && raw == "4")
            {
                return NumericFour;
            }
            if (value == 5 && raw == "5")
            {
                return NumericFive;
            }
            if (value == 6 && raw == "6")
            {
                return NumericSix;
            }
            if (value == 7 && raw == "7")
            {
                return NumericSeven;
            }
            if (value == 8 && raw == "8")
            {
                return NumericEight;
            }
            if (value == 9 && raw == "9")
            {
                return NumericNine;
            }
            if (value == 10 && raw == "10")
            {
                return NumericTen;
            }
            if (value == 11 && raw == "11")
            {
                return NumericEleven;
            }
            if (value == 100 && raw == "100")
            {
                return NumericHundred;
            }
            return new Literal(value, raw);
        }
        string PropertyKey.GetKey()
        {
            var s = Value as string;
            return s ?? Convert.ToString(Value);
        }
    }
}