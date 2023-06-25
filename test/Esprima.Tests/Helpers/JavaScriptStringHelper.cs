using System.Text;
using Esprima.Ast;

namespace Esprima.Tests.Helpers;

public class JavaScriptStringHelper
{
    private static readonly ParserOptions s_parserOptions = new() { AdaptRegexp = false, Tolerant = false };

    public static bool IsStringLiteral(string value) => value.Length > 2
        && (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)
            || value.StartsWith("'", StringComparison.Ordinal) && value.EndsWith("'", StringComparison.Ordinal));

    public static Expression ParseAsExpression(string value) => new JavaScriptParser(s_parserOptions).ParseExpression(value);

    public static string Decode(string value)
    {
        return IsStringLiteral(value)
            ? (string) ParseAsExpression(value).As<Literal>().Value!
            : throw new ArgumentException("Value is not a JavaScript string literal.", nameof(value));
    }

    public static string Encode(string value, bool addDoubleQuotes = true)
    {
        // A slightly modified version of HttpUtility.JavaScriptStringEncode, which doesn't escape HTML-sensitive chars but escapes lone surrogates.
        // Based on: https://github.com/dotnet/runtime/blob/v6.0.16/src/libraries/System.Web.HttpUtility/src/System/Web/Util/HttpEncoder.cs#L162

        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        StringBuilder? b = null;
        int startIndex = 0;
        int count = 0;
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            // Append the unhandled characters (that do not require special treament)
            // to the string builder when special characters are detected.
            if (CharRequiresJavaScriptEncoding(c)
                || char.IsHighSurrogate(c) && !char.IsLowSurrogate(value.CharCodeAt(i + 1))
                || char.IsLowSurrogate(c) && !char.IsHighSurrogate(value.CharCodeAt(i - 1)))
            {
                if (b == null)
                {
                    b = new StringBuilder(value.Length + 5);
                }

                if (count > 0)
                {
                    b.Append(value, startIndex, count);
                }

                startIndex = i + 1;
                count = 0;

                switch (c)
                {
                    case '\b':
                        b.Append(@"\b");
                        break;
                    case '\t':
                        b.Append(@"\t");
                        break;
                    case '\r':
                        b.Append(@"\r");
                        break;
                    case '\n':
                        b.Append(@"\n");
                        break;
                    case '\v':
                        b.Append(@"\v");
                        break;
                    case '\f':
                        b.Append(@"\f");
                        break;
                    case '\"':
                        b.Append(@"\""");
                        break;
                    case '\\':
                        b.Append(@"\\");
                        break;
                    default:
                        AppendCharAsUnicodeJavaScript(b, c);
                        break;
                }
            }
            else
            {
                count++;
            }
        }

        if (b == null)
        {
            return addDoubleQuotes ? "\"" + value + "\"" : value;
        }

        if (count > 0)
        {
            b.Append(value, startIndex, count);
        }

        return (addDoubleQuotes ? b.Insert(0, '"').Append('"') : b).ToString();

        static bool CharRequiresJavaScriptEncoding(char c) =>
            c < 0x20 || c == 0x7F
            || c == '\"'
            || c == '\\'
            || c == '\u0085'
            || c == '\u2028'
            || c == '\u2029';

        static void AppendCharAsUnicodeJavaScript(StringBuilder builder, char c)
        {
            if (c <= byte.MaxValue)
            {
                builder.Append(@$"\x{(int) c:x2}");
            }
            else
            {
                builder.Append(@$"\u{(int) c:x4}");
            }
        }
    }
}
