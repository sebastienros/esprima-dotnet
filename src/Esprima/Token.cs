using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Esprima.Ast;

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
    internal Token(
        TokenType type,
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
        Type = type;
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
    internal static Token Create(TokenType type, object? value, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(type, value, start, end, lineNumber, lineStart);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateStringLiteral(string str, bool octal, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.StringLiteral, str, start, end, lineNumber, lineStart, octal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateRegexLiteral(Regex? value, RegexValue regexValue, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.RegularExpression, value, start, end, lineNumber, lineStart, customValue: regexValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateNumericLiteral(double value, bool octal, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.NumericLiteral, value, start, end, lineNumber, lineStart, octal: octal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateBigIntLiteral(BigInteger value, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.BigIntLiteral, value, start, end, lineNumber, lineStart);
    }

    internal static Token CreateEof(int index, int lineNumber, int lineStart)
    {
        return new Token(TokenType.EOF, value: null, start: index, end: index, lineNumber, lineStart);
    }

    internal static Token CreatePunctuator(string str, int start, int end, int lineNumber, int lineStart)
    {
        return new Token(TokenType.Punctuator, str, start, end, lineNumber, lineStart);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Token CreateTemplate(
        string? cooked,
        string rawTemplate,
        bool head,
        bool tail,
        char notEscapeSequenceHead,
        int start,
        int end,
        int lineNumber,
        int lineStart)
    {
        return new Token(TokenType.Template, cooked, start, end, lineNumber, lineStart, notEscapeSequenceHead: notEscapeSequenceHead, customValue: rawTemplate, head: head, tail: tail);
    }

    public readonly TokenType Type;
    public readonly bool Octal;

    public readonly int Start; // Range[0]
    public readonly int End; // Range[1]
    public readonly int LineNumber;
    public readonly int LineStart;

    public readonly object? Value;

    public readonly char NotEscapeSequenceHead;
    public readonly bool Head;
    public readonly bool Tail;

    internal readonly object? _customValue;
    public string? RawTemplate { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Type == TokenType.Template ? (string?) _customValue : null; }
    public RegexValue? RegexValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Type == TokenType.RegularExpression ? (RegexValue?) _customValue : null; }

    internal Token ChangeType(TokenType newType)
    {
        return new Token(newType, Value, Start, End, LineNumber, LineStart, Octal, NotEscapeSequenceHead, Head, Tail, _customValue);
    }
}
