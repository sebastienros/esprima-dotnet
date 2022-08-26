using System.Runtime.CompilerServices;

namespace Esprima;

public enum JsxTokenType
{
    Unknown,
    Identifier,
    Text
}

public static class JsxToken
{
    internal sealed record JsxHolder(JsxTokenType Type, string Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateIdentifier(string value, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.Extension, new JsxHolder(Esprima.JsxTokenType.Identifier, value), start, end, lineNumber, lineStart);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateText(string value, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.Extension, new JsxHolder(Esprima.JsxTokenType.Text, value), start, end, lineNumber, lineStart);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsxTokenType JsxTokenType(this Token token) => token.Type == TokenType.Extension && token.Value is JsxHolder holder ? holder.Type : Esprima.JsxTokenType.Unknown;
}
