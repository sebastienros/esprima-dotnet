using System.Runtime.CompilerServices;

namespace Esprima;

internal static class ParserExtensions
{
    private static readonly string[] s_charToString = new string[256];

    static ParserExtensions()
    {
        for (var i = 0; i < s_charToString.Length; ++i)
        {
            var c = (char) i;
            s_charToString[i] = c.ToString();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string? TryGetInternedString(ReadOnlySpan<char> source)
    {
        return
            Scanner.TryGetInternedKeyword(source) ??
            Scanner.TryGetInternedContextualKeyword(source) ??
            Scanner.TryGetInternedStrictModeReservedWord(source) ??
            Scanner.TryGetInternedRestrictedWord(source) ??
            Scanner.TryGetInternedPunctuator(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string Slice(this string source, int start, int end, ref StringPool stringPool)
    {
        var sourceSpan = source.AsSpan(start, end - start);
        return TryGetInternedString(sourceSpan) ?? stringPool.GetOrCreate(sourceSpan);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string CharToString(char c)
    {
        if (c >= 0 && c < s_charToString.Length)
        {
            return s_charToString[c];
        }

        return c.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static char CharCodeAt(this string source, int index)
    {
        if (index < 0 || index > source.Length - 1)
        {
            // char.MinValue is used as the null value
            return char.MinValue;
        }

        return source[index];
    }
}
