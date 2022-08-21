using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Esprima;

internal static partial class ParserExtensions
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
    internal static ReadOnlySpan<char> Between(this string s, int start, int end)
    {
        return s.AsSpan(start, end - start);
    }

    [StringMatcher(
        // basic keywords (should include all keywords defined in Scanner.IsKeyword)
        "if", "in", "do", "var", "for", "new", "try", "let", "this", "else", "case", "void", "with", "enum",
        "while", "break", "catch", "throw", "const", "yield", "class", "super", "return", "typeof", "delete", "switch",
        "export", "import", "default", "finally", "extends", "function", "continue", "debugger", "instanceof",
        // contextual keywords (should at least include "null", "false" and "true")
        "as", "of", "get", "set", "false", "from", "null", "true", "async", "await", "static", "constructor",
        // some common identifiers/literals in our test data set (benchmarks + test suite)
        "undefined", "length", "object", "Object", "obj", "Array", "Math", "data", "done", "args", "arguments", "Symbol", "prototype",
        "options", "value", "name", "self", "key", "\"use strict\"", "use strict"
    )]
    internal static partial string? TryGetInternedString(ReadOnlySpan<char> source);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string ToInternedString(this ReadOnlySpan<char> source, ref StringPool stringPool)
    {
        if (source.Length == 1)
        {
            return CharToString(source[0]);
        }

        return TryGetInternedString(source) ?? stringPool.GetOrCreate(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string ToInternedString(this ReadOnlySpan<char> source, ref StringPool stringPool, int interningThreshold)
    {
        return source.Length <= interningThreshold ? source.ToInternedString(ref stringPool) : source.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string CharToString(char c)
    {
        int index = c;
        var temp = s_charToString;
        if ((uint) index < temp.Length)
        {
            return temp[index];
        }
        return c.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static char CharCodeAt(this string source, int index)
    {
        if ((uint) index < source.Length)
        {
            return source[index];
        }

        // char.MinValue is used as the null value
        return char.MinValue;
    }

    internal static bool TryConsumeInt(this ref ReadOnlySpan<char> s, out int result)
    {
        result = 0;
        char c;
        int i;
        for (i = 0; i < s.Length && (c = s[i]) is >= '0' and <= '9'; i++)
        {
            result = checked(result * 10 + c - '0');
        }

        if (i == 0)
        {
            result = default;
            return false;
        }

        s = s.Slice(i);
        return true;
    }
}
