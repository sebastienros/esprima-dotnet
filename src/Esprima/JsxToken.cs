using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;

namespace Esprima;

public enum JsxTokenType
{
    Unknown,
    Identifier,
    Text
}

public static class JsxToken
{
    private static readonly object s_boxedIdentifierTokenType = Esprima.JsxTokenType.Identifier;
    private static readonly object s_boxedTextTokenType = Esprima.JsxTokenType.Text;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateIdentifier(string source, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.Extension, source, null, start, end, lineNumber, lineStart, customValue: s_boxedIdentifierTokenType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateText(string source, string text, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.Extension, source, text, start, end, lineNumber, lineStart, customValue: s_boxedTextTokenType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static JsxTokenType JsxTokenType(this Token token) => token.Type == TokenType.Extension && token._customValue is JsxTokenType type ? type : Esprima.JsxTokenType.Unknown;
}
