using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;

namespace Esprima;

public static class ParserExtensions
{
    private static readonly string[] charToString = new string[256];

    static ParserExtensions()
    {
        for (var i = 0; i < charToString.Length; ++i)
        {
            var c = (char) i;
            charToString[i] = c.ToString();
        }
    }

    public static StringSegment Slice(this string source, int start, int end)
    {
        var len = source.Length;
        var from = start < 0 ? Math.Max(len + start, 0) : Math.Min(start, len);
        var to = end < 0 ? Math.Max(len + end, 0) : Math.Min(end, len);
        var span = Math.Max(to - from, 0);

        return new StringSegment(source, from, span);
    }

    public static StringSegment Slice(this StringSegment source, int start, int end)
    {
        var len = source.Length;
        var from = start < 0 ? Math.Max(len + start, 0) : Math.Min(start, len);
        var to = end < 0 ? Math.Max(len + end, 0) : Math.Min(end, len);
        var span = Math.Max(to - from, 0);

        return source.Subsegment(from, span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string CharToString(char c)
    {
        if (c >= 0 && c < charToString.Length)
        {
            return charToString[c];
        }

        return c.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char CharCodeAt(this string source, int index)
    {
        if (index < 0 || index > source.Length - 1)
        {
            // char.MinValue is used as the null value
            return char.MinValue;
        }

        return source[index];
    }
}