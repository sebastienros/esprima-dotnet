using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Esprima;

internal static partial class ParserExtensions
{
    // old framework doesn't know this flag
    private const int MethodImplOptionsAggressiveOptimization = 512;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly T Last<T>(this ReadOnlySpan<T> span) => ref span[span.Length - 1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Last<T>(this Span<T> span) => ref span[span.Length - 1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static
#if NETSTANDARD2_1_OR_GREATER
        ReadOnlySpan<char>
#else
        string
#endif
        ToParsable(this ReadOnlySpan<char> s)
    {
#if NETSTANDARD2_1_OR_GREATER
        return s;
#else
        return s.ToString();
#endif
    }

    internal static int FindIndex<T>(this ReadOnlySpan<T> s, Predicate<T> match, int startIndex = 0)
    {
        for (; startIndex < s.Length; startIndex++)
        {
            if (match(s[startIndex]))
            {
                return startIndex;
            }
        }
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int FindIndex(this string s, Predicate<char> match, int startIndex = 0)
    {
        return s.AsSpan().FindIndex(match, startIndex);
    }

    internal static string Replace(this string s, Predicate<char> match, char c)
    {
        var i = s.FindIndex(match);
        if (i < 0)
        {
            return s;
        }

#if !NETSTANDARD2_1_OR_GREATER
        var chars = s.ToCharArray();
#else
        return string.Create(s.Length, (s, i, match, c), static (chars, state) =>
        {
            var (s, i, match, c) = state;
            s.AsSpan().CopyTo(chars);
#endif

#pragma warning disable format
            chars[i++] = c;
            for (; i < chars.Length; i++)
            {
                if (match(chars[i]))
                {
                    chars[i] = c;
                }
            }
#pragma warning restore format

#if NETSTANDARD2_1_OR_GREATER
        });
#else
        return new string(chars);
#endif
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
    [MethodImpl((MethodImplOptions) MethodImplOptionsAggressiveOptimization)]
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

    internal static StringBuilder AppendCodePoint(this StringBuilder sb, int cp)
    {
        Debug.Assert(cp is >= 0 and <= Character.UnicodeLastCodePoint);

        if (cp > char.MaxValue)
        {
            Character.GetSurrogatePair((uint) cp, out var highSurrogate, out var lowSurrogate);
            return sb.Append(highSurrogate).Append(lowSurrogate);
        }

        return sb.Append((char) cp);
    }

    public static int CodePointAt(this string text, int i)
    {
        var ch = text.CharCodeAt(i);

        if (ch >= 0xD800 && ch <= 0xDBFF)
        {
            var ch2 = text.CharCodeAt(i + 1);
            if (ch2 >= 0xDC00 && ch2 <= 0xDFFF)
            {
                return (int) Character.GetCodePoint(ch, ch2);
            }
        }

        return ch;
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
