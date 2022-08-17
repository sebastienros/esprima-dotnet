using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Esprima.Ast;
using Microsoft.Extensions.Primitives;

namespace Esprima;

public enum TokenType : byte
{
    Unknown,
    BooleanLiteral,
    EOF,
    Identifier,
    Keyword,
    NullLiteral,
    NumericLiteral,
    Punctuator,
    StringLiteral,
    RegularExpression,
    Template,
    BigIntLiteral,

    Extension = byte.MaxValue
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct Token
{
    // the full source code of script/module
    private readonly string _source;

    internal Token(
        TokenType type,
        string source,
        object? value,
        int start,
        int end,
        int lineNumber,
        int lineStart,
        bool octal = false,
        char notEscapeSequenceHead = (char) 0,
        bool head = false,
        bool tail = false,
        object? customValue = null)
    {
        Debug.Assert(source.Length >= end - start);

        Type = type;
        _source = source;
        Octal = octal;
        Start = start;
        End = end;
        LineNumber = lineNumber;
        LineStart = lineStart;
        Value = value;
        NotEscapeSequenceHead = notEscapeSequenceHead;
        Head = head;
        Tail = tail;
        _customValue = customValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token Create(TokenType type, string source, object? value, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(type, source, value, start, end, lineNumber, lineStart);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateStringLiteral(string source, StringSegment str, bool octal, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.StringLiteral, source, str, start, end, lineNumber, lineStart, octal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateRegexLiteral(string source, Regex? value, RegexValue regexValue, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.RegularExpression, source, value, start, end, lineNumber, lineStart, customValue: regexValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateNumericLiteral(string source, double value, bool octal, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.NumericLiteral, source, value, start, end, lineNumber, lineStart, octal: octal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateBigIntLiteral(string source, BigInteger value, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.BigIntLiteral, source, value, start, end, lineNumber, lineStart);
    }

    internal static Token CreateEof(int index, int lineNumber, int lineStart)
    {
        return new Token(TokenType.EOF, source: "", value: null, start: index, end: index, lineNumber, lineStart);
    }

    internal static Token CreatePunctuator(string source, string value, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.Punctuator, source, value, start, end, lineNumber, lineStart);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateTemplate(
        string source,
        StringSegment? cooked,
        StringSegment rawTemplate,
        bool head,
        bool tail,
        char notEscapeSequenceHead,
        int start,
        int end,
        int lineNumber,
        int lineStart)
    {
        return new Token(TokenType.Template, source, cooked, start, end, lineNumber, lineStart, notEscapeSequenceHead: notEscapeSequenceHead, customValue: rawTemplate, head: head, tail: tail);
    }

    public readonly TokenType Type;
    public readonly bool Octal;

    internal ReadOnlySpan<char> Source => Start == End ? ReadOnlySpan<char>.Empty : _source.AsSpan(Start, End - Start);

    internal StringSegment GetSegment()
    {
        return new StringSegment(_source, Start, End - Start);
    }

    public readonly int Start; // Range[0]
    public readonly int End; // Range[1]
    public readonly int LineNumber;
    public readonly int LineStart;

    public readonly object? Value;

    public readonly char NotEscapeSequenceHead;
    public readonly bool Head;
    public readonly bool Tail;

    internal readonly object? _customValue;
    public StringSegment? RawTemplate { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Type == TokenType.Template ? (StringSegment?) _customValue : null; }
    public RegexValue? RegexValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Type == TokenType.RegularExpression ? (RegexValue?) _customValue : null; }

    internal Token ChangeType(TokenType newType)
    {
        return new Token(newType, _source, Value, Start, End, LineNumber, LineStart, Octal, NotEscapeSequenceHead, Head, Tail, _customValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsPunctuator(string punctuator) => Type == TokenType.Punctuator && Source.SequenceEqual(punctuator.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsKeyword(string keyword) => Type == TokenType.Keyword && Source.SequenceEqual(keyword.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsIdentifier(string identifier) => Type == TokenType.Identifier && Source.SequenceEqual(identifier.AsSpan());
}
