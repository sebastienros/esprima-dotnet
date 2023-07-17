using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Esprima.Ast;

namespace Esprima;

internal readonly struct ScannerState
{
    public readonly int Index;
    public readonly int LineNumber;
    public readonly int LineStart;
    public readonly ArrayList<string> CurlyStack;

    public ScannerState(int index, int lineNumber, int lineStart, ArrayList<string> curlyStack)
    {
        Index = index;
        LineNumber = lineNumber;
        LineStart = lineStart;
        CurlyStack = curlyStack;
    }
}

internal readonly record struct LexOptions(bool Strict, bool AllowIdentifierEscape)
{
    public LexOptions(JavaScriptParser.Context context) : this(context.Strict, context.AllowIdentifierEscape)
    {
    }
}

public sealed partial class Scanner
{
    internal const int NonIdentifierInterningThreshold = 20;

    private readonly ErrorHandler _errorHandler;
    private readonly bool _tolerant;
    private readonly bool _trackComment;
    private readonly RegExpParseMode _regExpParseMode;
    private readonly TimeSpan _regexTimeout;

    private int _length;
    internal string _source; // should be named _code to match the corresponding ctor parameter name but internally we keep this name to match Esprima.org naming
    internal string? _sourceLocation; // should be named _source to match the corresponding ctor parameter name

    internal int _index;
    internal int _lineNumber;
    internal int _lineStart;

    internal bool _isModule;

    private ArrayList<string> _curlyStack;
    private StringBuilder _sb = new();

    internal StringPool _stringPool;
    internal CodePointRange.Cache? _codePointRangeCache;

    private static int OctalValue(char ch)
    {
        return ch - '0';
    }

    internal Scanner(ScannerOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _regExpParseMode = options.RegExpParseMode;
        _regexTimeout = options.RegexTimeout;
        _errorHandler = options.ErrorHandler;
        _tolerant = options.Tolerant;
        _trackComment = options.Comments;

        _source = string.Empty;
    }

    public Scanner(string code) : this(code, ScannerOptions.Default)
    {
    }

    public Scanner(string code, ScannerOptions options) : this(code, null, options)
    {
    }

    public Scanner(string code, string? source) : this(code, source, ScannerOptions.Default)
    {
    }

    public Scanner(string code, string? source, ScannerOptions options) : this(options)
    {
        Reset(code, source);
    }

    public void Reset()
    {
        Reset(startIndex: 0, lineNumber: _length > 0 ? 1 : 0, lineStartIndex: 0);
    }

    public void Reset(int startIndex, int lineNumber, int lineStartIndex)
    {
        if (_length > 0)
        {
            _index = 0 <= startIndex && startIndex < _length ? startIndex : throw new ArgumentOutOfRangeException(nameof(startIndex));
            _lineNumber = lineNumber > 0 ? lineNumber : throw new ArgumentOutOfRangeException(nameof(lineNumber));
            _lineStart = 0 <= lineStartIndex && lineStartIndex <= _index ? lineStartIndex : throw new ArgumentOutOfRangeException(nameof(lineStartIndex));
        }
        else
        {
            _index = startIndex == 0 ? startIndex : throw new ArgumentOutOfRangeException(nameof(startIndex));
            _lineNumber = lineNumber == 0 ? lineNumber : throw new ArgumentOutOfRangeException(nameof(lineNumber));
            _lineStart = lineStartIndex == 0 ? lineStartIndex : throw new ArgumentOutOfRangeException(nameof(lineStartIndex));
        }

        _curlyStack.Clear();
        _sb.Clear();

        _errorHandler.Reset();
    }

    internal void Reset(string code, string? source)
    {
        Reset(code, source, startIndex: 0, lineNumber: code.Length > 0 ? 1 : 0, lineStartIndex: 0);
    }

    internal void Reset(string code, string? source, int startIndex, int lineNumber, int lineStartIndex)
    {
        _source = code ?? throw new ArgumentNullException(nameof(code));
        _length = code.Length;
        _sourceLocation = source;

        Reset(startIndex, lineNumber, lineStartIndex);
        _stringPool = default;
        _codePointRangeCache = null;
        _isModule = false;
    }

    internal void ReleaseLargeBuffers()
    {
        _curlyStack.Clear();
        if (_curlyStack.Capacity > 16)
        {
            _curlyStack.Capacity = 16;
        }

        _sb.Clear();
        if (_sb.Capacity > 1024)
        {
            _sb.Capacity = 1024;
        }

        _stringPool = default;
        _codePointRangeCache = null;
    }

    public string Code { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _source; }

    public int Index { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _index; }
    public int LineNumber { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _lineNumber; }
    public int LineStart { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _lineStart; }

    internal ScannerState SaveState()
    {
        return new ScannerState(_index, _lineNumber, _lineStart, new ArrayList<string>(_curlyStack.ToArray()));
    }

    internal void RestoreState(in ScannerState state)
    {
        _index = state.Index;
        _lineNumber = state.LineNumber;
        _lineStart = state.LineStart;
        _curlyStack = state.CurlyStack;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool Eof()
    {
        return _index >= _length;
    }

    [DoesNotReturn]
    private void ThrowUnexpectedToken(string message = Messages.UnexpectedTokenIllegal)
    {
        throw _errorHandler.CreateError(_sourceLocation, _index, _lineNumber, _index - _lineStart, message).ToException();
    }

    [DoesNotReturn]
    private T ThrowUnexpectedToken<T>(string message = Messages.UnexpectedTokenIllegal)
    {
        throw _errorHandler.CreateError(_sourceLocation, _index, _lineNumber, _index - _lineStart, message).ToException();
    }

    private void TolerateUnexpectedToken(string message = Messages.UnexpectedTokenIllegal)
    {
        _errorHandler.TolerateError(_sourceLocation, _index, _lineNumber, _index - _lineStart, message, _tolerant);
    }

    private StringBuilder GetStringBuilder()
    {
        _sb.Clear();
        return _sb;
    }

    // https://tc39.github.io/ecma262/#sec-future-reserved-words

    [StringMatcher("enum", "export", "import", "super")]
    public static partial bool IsFutureReservedWord(string id);

    [StringMatcher("implements", "interface", "package", "private", "protected", "public", "static", "yield", "let")]
    public static partial bool IsStrictModeReservedWord(string id);

    public static bool IsRestrictedWord(string id)
    {
        return id is "eval" or "arguments";
    }

    [StringMatcher("&&", "||", "==", "!=", "+=", "-=", "*=", "/=", "++", "--", "<<", ">>", "&=", "|=", "^=", "%=", "<=", ">=", "=>", "**")]
    private static partial string? TryGetInternedTwoCharacterPunctuator(ReadOnlySpan<char> id);

    [StringMatcher("===", "!==", ">>>", "<<=", ">>=", "**=", "&&=", "||=")]
    private static partial string? TryGetInternedThreeCharacterPunctuator(ReadOnlySpan<char> id);

    // https://tc39.github.io/ecma262/#sec-keywords

    // Note for maintainers: all keywords listed here should be included in ParserExtensions.TryGetInternedString too!
    [StringMatcher(
        "if", "in", "do", "var", "for", "new", "try", "let", "this", "else", "case", "void", "with", "enum",
        "while", "break", "catch", "throw", "const", "yield", "class", "super", "return", "typeof", "delete", "switch",
        "export", "import", "default", "finally", "extends", "function", "continue", "debugger", "instanceof")]
    public static partial bool IsKeyword(string id);

    // https://tc39.github.io/ecma262/#sec-comments

    private ArrayList<Comment> SkipSingleLineComment(int offset)
    {
        var comments = new ArrayList<Comment>();
        var start = 0;
        Position startPosition = default, endPosition;
        Esprima.Range slice;

        if (_trackComment)
        {
            start = _index - offset;
            startPosition = new Position(_lineNumber, _index - _lineStart - offset);
        }

        while (!Eof())
        {
            var ch = _source[_index];
            ++_index;
            if (Character.IsLineTerminator(ch))
            {
                if (_trackComment)
                {
                    endPosition = new Position(_lineNumber, _index - _lineStart - 1);
                    slice = new Esprima.Range(start + offset, _index - 1);
                    var entry = new Comment
                    (
                        type: CommentType.Line,
                        slice,
                        start: start,
                        end: _index - 1,
                        startPosition,
                        endPosition
                    );

                    comments.Add(entry);
                }

                if (ch == 13 && _source.CharCodeAt(_index) == 10)
                {
                    ++_index;
                }

                ++_lineNumber;
                _lineStart = _index;
                return comments;
            }
        }

        if (_trackComment)
        {
            endPosition = new Position(_lineNumber, _index - _lineStart);
            slice = new Esprima.Range(start + offset, _index);
            var entry = new Comment
            (
                type: CommentType.Line,
                slice,
                start: start,
                end: _index,
                startPosition,
                endPosition
            );

            comments.Add(entry);
        }

        return comments;
    }

    private ArrayList<Comment> SkipMultiLineComment()
    {
        var comments = new ArrayList<Comment>();
        var start = 0;
        Position startPosition = default, endPosition;
        Esprima.Range slice;

        if (_trackComment)
        {
            start = _index - 2;
            startPosition = new Position(_lineNumber, _index - _lineStart - 2);
        }

        while (!Eof())
        {
            var ch = _source[_index];
            if (Character.IsLineTerminator(ch))
            {
                if (ch == 0x0D && _source.CharCodeAt(_index + 1) == 0x0A)
                {
                    ++_index;
                }

                ++_lineNumber;
                ++_index;
                _lineStart = _index;
            }
            else if (ch == 0x2A)
            {
                // Block comment ends with '*/'.
                if (_source.CharCodeAt(_index + 1) == 0x2F)
                {
                    _index += 2;
                    if (_trackComment)
                    {
                        endPosition = new Position(_lineNumber, _index - _lineStart);
                        slice = new Esprima.Range(start + 2, _index - 2);
                        var entry = new Comment
                        (
                            type: CommentType.Block,
                            slice,
                            start: start,
                            end: _index,
                            startPosition,
                            endPosition
                        );
                        comments.Add(entry);
                    }

                    return comments;
                }

                ++_index;
            }
            else
            {
                ++_index;
            }
        }

        // Ran off the end of the file - the whole thing is a comment
        if (_trackComment)
        {
            endPosition = new Position(_lineNumber, _index - _lineStart);
            slice = new Esprima.Range(start + 2, _index);
            var entry = new Comment
            (
                type: CommentType.Block,
                slice,
                start: start,
                end: _index,
                startPosition,
                endPosition
            );
            comments.Add(entry);
        }

        TolerateUnexpectedToken();
        return comments;
    }

    public ReadOnlySpan<Comment> ScanComments()
    {
        var comments = new ArrayList<Comment>();

        var start = _index == 0;
        while (!Eof())
        {
            var ch = _source[_index];

            if (Character.IsWhiteSpace(ch))
            {
                ++_index;
            }
            else if (Character.IsLineTerminator(ch))
            {
                ++_index;
                if (ch == 0x0D && _source.CharCodeAt(_index) == 0x0A)
                {
                    ++_index;
                }

                ++_lineNumber;
                _lineStart = _index;
                start = true;
            }
            else if (ch == 0x2F)
            {
                // U+002F is '/'
                ch = _source.CharCodeAt(_index + 1);
                if (ch == 0x2F)
                {
                    _index += 2;
                    var comment = SkipSingleLineComment(2);
                    if (_trackComment)
                    {
                        comments.AddRange(comment);
                    }

                    start = true;
                }
                else if (ch == 0x2A)
                {
                    // U+002A is '*'
                    _index += 2;
                    var comment = SkipMultiLineComment();
                    if (_trackComment)
                    {
                        comments.AddRange(comment);
                    }
                }
                else
                {
                    break;
                }
            }
            else if (start && ch == 0x2D)
            {
                // U+002D is '-'
                // U+003E is '>'
                if (_source.CharCodeAt(_index + 1) == 0x2D && _source.CharCodeAt(_index + 2) == 0x3E)
                {
                    // '-->' is a single-line comment
                    _index += 3;
                    var comment = SkipSingleLineComment(3);
                    if (_trackComment)
                    {
                        comments.AddRange(comment);
                    }
                }
                else
                {
                    break;
                }
            }
            else if (ch == 0x3C)
            {
                // U+003C is '<'
                if (_source[_index + 1] == '!'
                    && _source[_index + 2] == '-'
                    && _source[_index + 3] == '-')
                {
                    if (_isModule)
                    {
                        ThrowUnexpectedToken();
                    }

                    _index += 4; // `<!--`
                    var comment = SkipSingleLineComment(4);
                    if (_trackComment)
                    {
                        comments.AddRange(comment);
                    }
                }
                else
                {
                    break;
                }
            }
            else if (_index == 0 && ch == '#')
            {
                if (_source[_index + 1] != '!')
                {
                    ThrowUnexpectedToken();
                }

                // hashbang
                _index += 2;
                while (!Eof())
                {
                    ch = _source.CharCodeAt(_index);
                    if (ch == '/' && _source.CharCodeAt(_index + 1) == '*')
                    {
                        ThrowUnexpectedToken();
                    }

                    _index++;
                    if (Character.IsLineTerminator(ch))
                    {
                        break;
                    }
                }
            }
            else
            {
                break;
            }
        }

        return comments.AsSpan();
    }

    public static int CodePointAt(string text, int i)
    {
        int cp = text.CharCodeAt(i);

        if (cp >= 0xD800 && cp <= 0xDBFF)
        {
            var second = text.CharCodeAt(i + 1);
            if (second >= 0xDC00 && second <= 0xDFFF)
            {
                var first = cp;
                cp = (first - 0xD800) * 0x400 + second - 0xDC00 + 0x10000;
            }
        }

        return cp;

        // There seems to be a bug with "\ud800" when using char.ConvertToUtf32(text, i);
        // Test: /[\\uD800-\\uFA6D]/u
    }

    private bool ScanHexEscape(char prefix, out char result)
    {
        var len = prefix == 'u' ? 4 : 2;
        var code = 0;

        for (var i = 0; i < len; ++i)
        {
            if (!Eof())
            {
                var d = _source[_index];
                if (Character.IsHexDigit(d))
                {
                    code = code * 16 + HexConverter.FromChar(d);
                    _index++;
                }
                else
                {
                    result = char.MinValue;
                    return false;
                }
            }
            else
            {
                result = char.MinValue;
                return false;
            }
        }

        result = (char) code;
        return true;
    }

    private bool TryScanUnicodeCodePointEscape(out int code)
    {
        var ch = _source.CharCodeAt(_index);
        code = 0;

        // At least, one hex digit is required.
        if (ch == '}')
        {
            return false;
        }

        while (!Eof())
        {
            ch = _source[_index++];
            if (!Character.IsHexDigit(ch))
            {
                break;
            }

            code = code * 16 + HexConverter.FromChar(ch);

            // The Unicode standard guarantees that a code point above 0x10FFFF will never be assigned
            // (see https://stackoverflow.com/a/52203901/8656352).
            if (code > Character.UnicodeLastCodePoint)
            {
                return false;
            }
        }

        if (ch != '}')
        {
            return false;
        }

        // The surrogate range is valid in literals (e.g. "a\u{d800}\u{dc00}") but not valid in identifiers (e.g. a\u{d800}\u{dc00}).
        // Let's return true in both cases and let the caller deal with it.

        return true;
    }

    private int ScanUnicodeCodePointEscape()
    {
        if (!TryScanUnicodeCodePointEscape(out var cp))
        {
            ThrowUnexpectedToken(cp > Character.UnicodeLastCodePoint
                ? Messages.UndefinedUnicodeCodePoint
                : Messages.InvalidUnicodeEscapeSequence);
        }

        return cp;
    }

    private string GetIdentifier(bool allowEscapedSurrogates = false)
    {
        var start = _index++;
        while (!Eof())
        {
            var ch = _source[_index];
            if ((ushort) ch is
                0x5C // Blackslash (U+005C) marks Unicode escape sequence.
                or >= 0xD800 and <= 0xDFFF) // Need to handle surrogate pairs. 
            {
                _index = start;
                return GetComplexIdentifier(allowEscapedSurrogates);
            }

            if (Character.IsIdentifierPart(ch))
            {
                ++_index;
            }
            else
            {
                break;
            }
        }

        return _source.Between(start, _index).ToInternedString(ref _stringPool);
    }

    private string GetComplexIdentifier(bool allowEscapedSurrogates = false)
    {
        var sb = GetStringBuilder();

        var cp = CodePointAt(_source, _index);
        string ch;
        int chcp;

        if (cp != 0x5C)
        {
            if (cp <= char.MaxValue)
            {
                if (char.IsSurrogate((char) cp))
                {
                    ThrowUnexpectedToken();
                }

                sb.Append((char) cp);
                _index++;
            }
            else
            {
                if (!Character.IsIdentifierStartAstral(cp))
                {
                    ThrowUnexpectedToken();
                }

                ch = ParserExtensions.CodePointToString(cp);
                sb.Append(ch);
                _index += ch.Length;
            }
        }
        else
        {
            // '\u' (U+005C, U+0075) denotes an escaped character.
            ++_index;
            if (_source.CharCodeAt(_index) != 0x75)
            {
                ThrowUnexpectedToken();
            }

            ++_index;
            if (_source.CharCodeAt(_index) == '{')
            {
                ++_index;
                if (!TryScanUnicodeCodePointEscape(out chcp))
                {
                    ThrowUnexpectedToken(chcp > Character.UnicodeLastCodePoint
                        ? Messages.UndefinedUnicodeCodePoint
                        : Messages.InvalidUnicodeEscapeSequence);
                }
            }
            else
            {
                if (!ScanHexEscape('u', out var ch1) || ch1 == '\\')
                {
                    ThrowUnexpectedToken();
                }

                if (!char.IsSurrogate(ch1))
                {
                    if (!Character.IsIdentifierStart(ch1))
                    {
                        ThrowUnexpectedToken();
                    }

                    sb.Append(ch1);
                    goto ParseIdentifierPart;
                }
                else if (!allowEscapedSurrogates || !char.IsHighSurrogate(ch1) || !TryGetEscapedSurrogate(ch1, out chcp))
                {
                    ThrowUnexpectedToken();
                    chcp = default; // keeps the compiler happy
                }
            }

            if (chcp > char.MaxValue)
            {
                if (!Character.IsIdentifierStartAstral(chcp))
                {
                    ThrowUnexpectedToken();
                }
                sb.Append(ParserExtensions.CodePointToString(chcp));
            }
            else
            {
                if (char.IsSurrogate((char) chcp) || !Character.IsIdentifierStart((char) chcp))
                {
                    ThrowUnexpectedToken();
                }
                sb.Append((char) chcp);
            }
        }

ParseIdentifierPart:
        while (!Eof())
        {
            cp = CodePointAt(_source, _index);

            if (cp != 0x5C)
            {
                if (cp <= char.MaxValue)
                {
                    // IsIdentifierPart also matches the surrogate range (U+D800..U+DFFF).
                    if (!Character.IsIdentifierPart((char) cp))
                    {
                        break;
                    }
                    else if (char.IsSurrogate((char) cp))
                    {
                        ThrowUnexpectedToken();
                    }

                    sb.Append((char) cp);
                    _index++;
                }
                else
                {
                    if (!Character.IsIdentifierPartAstral(cp))
                    {
                        break;
                    }

                    ch = ParserExtensions.CodePointToString(cp);
                    sb.Append(ch);
                    _index += ch.Length;
                }
            }
            else
            {
                // '\u' (U+005C, U+0075) denotes an escaped character.
                ++_index;
                if (_source.CharCodeAt(_index) != 0x75)
                {
                    ThrowUnexpectedToken();
                }

                ++_index;
                if (_source.CharCodeAt(_index) == '{')
                {
                    ++_index;
                    if (!TryScanUnicodeCodePointEscape(out chcp))
                    {
                        ThrowUnexpectedToken(chcp > Character.UnicodeLastCodePoint
                            ? Messages.UndefinedUnicodeCodePoint
                            : Messages.InvalidUnicodeEscapeSequence);
                    }
                }
                else
                {
                    if (!ScanHexEscape('u', out var ch1) || ch1 == '\\')
                    {
                        ThrowUnexpectedToken();
                    }

                    if (!char.IsSurrogate(ch1))
                    {
                        if (!Character.IsIdentifierPart(ch1))
                        {
                            ThrowUnexpectedToken();
                        }

                        sb.Append(ch1);
                        continue;
                    }
                    else if (!allowEscapedSurrogates || !char.IsHighSurrogate(ch1) || !TryGetEscapedSurrogate(ch1, out chcp))
                    {
                        ThrowUnexpectedToken();
                        chcp = default; // keeps the compiler happy
                    }
                }

                if (chcp > char.MaxValue)
                {
                    if (!Character.IsIdentifierPartAstral(chcp))
                    {
                        ThrowUnexpectedToken();
                    }
                    sb.Append(ParserExtensions.CodePointToString(chcp));
                }
                else
                {
                    if (char.IsSurrogate((char) chcp) || !Character.IsIdentifierPart((char) chcp))
                    {
                        ThrowUnexpectedToken();
                    }
                    sb.Append((char) chcp);
                }
            }
        }

        return sb.ToString().AsSpan().ToInternedString(ref _stringPool);
    }

    private bool TryGetEscapedSurrogate(char highSurrogate, out int cp)
    {
        if (_source[_index] == '\\' && _source.CharCodeAt(_index + 1) == 'u')
        {
            _index += 2;
            if (ScanHexEscape('u', out var lowSurrogate) && char.IsLowSurrogate(lowSurrogate))
            {
                cp = char.ConvertToUtf32(highSurrogate, lowSurrogate);
                return true;
            }
        }

        cp = default;
        return false;
    }

    private OctalValue OctalToDecimal(char ch)
    {
        // \0 is not octal escape sequence
        var octal = ch != '0';
        var code = OctalValue(ch);

        if (!Eof() && Character.IsOctalDigit(_source[_index]))
        {
            octal = true;
            code = code * 8 + OctalValue(_source[_index++]);

            // 3 digits are only allowed when string starts
            // with 0, 1, 2, 3
            if (ch >= '0' && ch <= '3' && !Eof() && Character.IsOctalDigit(_source.CharCodeAt(_index)))
            {
                code = code * 8 + OctalValue(_source[_index++]);
            }
        }

        return new OctalValue(code, octal);
    }

    // https://tc39.github.io/ecma262/#sec-names-and-keywords

    private Token ScanIdentifier(bool allowEscapes)
    {
        TokenType type;
        var start = _index;

        // Backslash (U+005C) starts an escaped character.
        var id = (ushort) _source[_index] is 0x5C or (>= 0xD800 and <= 0xDFFF)
            ? GetComplexIdentifier()
            : GetIdentifier();

        // There is no keyword or literal with only one character.
        // Thus, it must be an identifier.
        if (id.Length == 1)
        {
            type = TokenType.Identifier;
        }
        else if (IsKeyword(id))
        {
            type = TokenType.Keyword;
        }
        else if ("null".Equals(id))
        {
            type = TokenType.NullLiteral;
        }
        else if ("true".Equals(id) || "false".Equals(id))
        {
            type = TokenType.BooleanLiteral;
        }
        else
        {
            type = TokenType.Identifier;
        }

        if (type != TokenType.Identifier && start + id.Length != _index)
        {
            var restore = _index;
            _index = start;
            if (!allowEscapes)
            {
                TolerateUnexpectedToken(Messages.InvalidEscapedReservedWord);
            }
            _index = restore;
        }

        return Token.Create(type, id, start, end: _index, _lineNumber, _lineStart);
    }

    // https://tc39.github.io/ecma262/#sec-punctuators

    private Token ScanPunctuator()
    {
        var start = _index;

        // Check for most common single-character punctuators.
        var c = _source[_index];
        var str = ParserExtensions.CharToString(c);

        switch (c)
        {
            case '(':
                ++_index;
                break;

            case '{':
                _curlyStack.Add("{");
                ++_index;
                break;

            case '.':
                ++_index;
                if (_source.Length >= _index + 2 && _source[_index] == '.' && _source[_index + 1] == '.')
                {
                    // Spread operator: ...
                    _index += 2;
                    str = "...";
                }

                break;

            case '}':
                ++_index;
                if (_curlyStack.Count > 0)
                {
                    _curlyStack.RemoveAt(_curlyStack.Count - 1);
                }

                break;

            case '?':
                ++_index;
                if (_source[_index] == '?')
                {
                    ++_index;
                    if (_source[_index] == '=')
                    {
                        ++_index;
                        str = "??=";
                    }
                    else
                    {
                        str = "??";
                    }
                }

                if (_source[_index] == '.' && !char.IsDigit(_source[_index + 1]))
                {
                    // "?." in "foo?.3:0" should not be treated as optional chaining.
                    // See https://github.com/tc39/proposal-optional-chaining#notes
                    ++_index;
                    str = "?.";
                }

                break;

            case '#':
                ++_index;
                if (_source.Length >= _index + 1 && _source[_index] == '!')
                {
                    _index += 1;
                    str = "#!";
                }
                break;
            case ')':
            case ';':
            case ',':
            case '[':
            case ']':
            case ':':
            case '~':
            case '@':
                ++_index;
                break;

            default:
                // 4-character punctuator.
                if (_index + 4 <= _source.Length && _source.AsSpan(_index, 4).SequenceEqual(">>>=".AsSpan()))
                {
                    _index += 4;
                    str = ">>>=";
                }
                // 3-character punctuators.
                else if (_index + 3 <= _source.Length && (str = TryGetInternedThreeCharacterPunctuator(_source.AsSpan(_index, 3))) is not null)
                {
                    _index += 3;
                }
                // 2-character punctuators.
                else if (_index + 2 <= _source.Length && (str = TryGetInternedTwoCharacterPunctuator(_source.AsSpan(_index, 2))) is not null)
                {
                    _index += 2;
                }
                // 1-character punctuators.
                else
                {
                    str = ParserExtensions.CharToString(c);
                    if ("<>=!+-*%&|^/".IndexOf(c) >= 0)
                    {
                        ++_index;
                    }
                }

                break;
        }

        if (_index == start)
        {
            ThrowUnexpectedToken();
        }

        return Token.CreatePunctuator(str, start, end: _index, _lineNumber, _lineStart);
    }

    // https://tc39.github.io/ecma262/#sec-literals-numeric-literals

    private Token ScanHexLiteral(int start)
    {
        var number = this.ScanLiteralPart(Character.IsHexDigitFunc, allowNumericSeparator: true, out var hasUpperCase);

        if (number.Length == 0)
        {
            ThrowUnexpectedToken();
        }

        var ch = _source.CharCodeAt(_index);
        if (Character.IsIdentifierStart(ch))
        {
            if (ch == 'n')
            {
                _index++;
                return ScanBigIntLiteral(start, number, JavaScriptNumberStyle.Hex);
            }
            ThrowUnexpectedToken();
        }

        double value = 0;

        if (number.Length < 16)
        {
#if !HAS_SPAN_PARSE
            value = Convert.ToInt64(number.ToString(), 16);
#else
            value = long.Parse(number, NumberStyles.AllowHexSpecifier, null);
#endif
        }
        else if (number.Length > 255)
        {
            value = double.PositiveInfinity;
        }
        else
        {
            double modulo = 1;
            var literal = hasUpperCase ? number.ToString().ToLowerInvariant().AsSpan() : number;
            var length = literal.Length - 1;
            for (var i = length; i >= 0; i--)
            {
                var c = literal[i];

                if (c <= '9')
                {
                    value += modulo * (c - '0');
                }
                else
                {
                    value += modulo * (c - 'a' + 10);
                }

                modulo *= 16;
            }
        }

        return Token.CreateNumericLiteral(value, octal: false, start, end: _index, _lineNumber, _lineStart);
    }

    private enum JavaScriptNumberStyle
    {
        Binary,
        Hex,
        Octal,
        Integer
    }

    private Token ScanBigIntLiteral(int start, ReadOnlySpan<char> number, JavaScriptNumberStyle style)
    {
        BigInteger bigInt = 0;
        if (style == JavaScriptNumberStyle.Binary)
        {
            // binary
            foreach (var c in number)
            {
                bigInt <<= 1;
                bigInt += c == '1' ? BigInteger.One : BigInteger.Zero;
            }
        }
        else
        {
            if (style == JavaScriptNumberStyle.Hex)
            {
                var c = number[0];
                if (c > '7' && Character.IsHexDigit(c))
                {
                    // ensure we get positive number
                    number = ("0" + number.ToString()).AsSpan();
                }
            }

            var parseStyle = style switch
            {
                JavaScriptNumberStyle.Integer => NumberStyles.None,
                JavaScriptNumberStyle.Hex => NumberStyles.AllowHexSpecifier,
                _ => NumberStyles.None
            };
#if HAS_SPAN_PARSE
            bigInt = BigInteger.Parse(number, parseStyle);
#else
            bigInt = BigInteger.Parse(number.ToString(), parseStyle);
#endif
        }

        return Token.CreateBigIntLiteral(bigInt, start, end: _index, _lineNumber, _lineStart);
    }

    private Token ScanBinaryLiteral(int start)
    {
        var number = this.ScanLiteralPart(static c => c is '0' or '1', allowNumericSeparator: true, out _);

        if (number.Length == 0)
        {
            // only 0b or 0B
            ThrowUnexpectedToken();
        }

        if (!Eof())
        {
            var ch = _source[_index];
            if (ch == 'n')
            {
                _index++;
                return ScanBigIntLiteral(start, number, JavaScriptNumberStyle.Binary);
            }

            if (Character.IsIdentifierStart(ch) || Character.IsDecimalDigit(ch))
            {
                ThrowUnexpectedToken();
            }
        }

        var numberString = number.ToString();
        var value = Convert.ToUInt32(numberString, 2);
        return Token.CreateNumericLiteral(value, octal: false, start, end: _index, _lineNumber, _lineStart);
    }

    private Token ScanOctalLiteral(char prefix, int start, bool isLegacyOctalDigital = false)
    {
        var sb = GetStringBuilder();
        var octal = false;

        if (Character.IsOctalDigit(prefix))
        {
            octal = true;
            sb.Append("0").Append(_source[_index++]);
        }
        else
        {
            ++_index;
        }

        sb.Append(this.ScanLiteralPart(Character.IsOctalDigitFunc, allowNumericSeparator: true, out _));
        var number = sb.ToString();

        if (!octal && number.Length == 0)
        {
            // only 0o or 0O
            ThrowUnexpectedToken();
        }

        var ch = _source.CharCodeAt(_index);

        if (ch == 'n')
        {
            if (isLegacyOctalDigital)
            {
                ThrowUnexpectedToken();
            }

            _index++;
            return ScanBigIntLiteral(start, number.AsSpan(), JavaScriptNumberStyle.Octal);
        }

        if (Character.IsIdentifierStart(ch) || Character.IsDecimalDigit(ch))
        {
            ThrowUnexpectedToken();
        }

        ulong numericValue;
        try
        {
            numericValue = Convert.ToUInt64(number, 8);
        }
        catch (OverflowException)
        {
            return ThrowUnexpectedToken<Token>($"Value {number} was either too large or too small for a UInt64.");
        }

        return Token.CreateNumericLiteral(numericValue, octal, start, end: _index, _lineNumber, _lineStart);
    }

    private bool IsImplicitOctalLiteral()
    {
        // Implicit octal, unless there is a non-octal digit.
        // (Annex B.1.1 on Numeric Literals)
        for (var i = _index + 1; i < _length; ++i)
        {
            var ch = _source[i];
            if (ch == '8' || ch == '9')
            {
                return false;
            }

            if (!Character.IsOctalDigit(ch))
            {
                return true;
            }
        }

        return true;
    }

    private ReadOnlySpan<char> ScanLiteralPart(Func<char, bool> check, bool allowNumericSeparator, out bool hasUpperCase)
    {
        hasUpperCase = false;
        var start = _index;

        var charCode = _source.CharCodeAt(_index);
        if (charCode == '_')
        {
            ThrowUnexpectedToken(Messages.NumericSeparatorNotAllowedHere);
        }

        var needsCleanup = false;
        while (check(charCode) || charCode == '_')
        {
            if (charCode == '_')
            {
                if (!allowNumericSeparator)
                {
                    ThrowUnexpectedToken();
                }
                needsCleanup = true;
            }

            hasUpperCase |= char.IsUpper(charCode);

            _index++;
            var newCharCode = _source.CharCodeAt(_index);
            if (charCode == '_')
            {
                if (newCharCode == '_')
                {
                    ThrowUnexpectedToken(Messages.NumericSeparatorOneUnderscore);
                }

                if (newCharCode == 'n')
                {
                    ThrowUnexpectedToken(Messages.NumericSeparatorNotAllowedHere);
                }
            }

            if (Eof())
            {
                break;
            }
            charCode = newCharCode;
        }

        if (_source[_index - 1] == '_')
        {
            ThrowUnexpectedToken(Messages.NumericSeparatorNotAllowedHere);
        }

        var span = _source.AsSpan(start, _index - start);
        return needsCleanup
            ? span.ToString().Replace("_", "").AsSpan()
            : span;
    }

    private Token ScanNumericLiteral(bool strict)
    {
        var sb = GetStringBuilder();
        var start = _index;
        var ch = _source[start];
        //assert(Character.IsDecimalDigit(ch) || (ch == '.'),
        //    'Numeric literal must start with a decimal digit or a decimal point');

        var nonOctal = false;
        if (ch != '.')
        {
            var first = _source[_index++];
            ch = _source.CharCodeAt(_index);

            // Hex number starts with '0x'.
            // Octal number starts with '0'.
            // Octal number in ES6 starts with '0o'.
            // Binary number in ES6 starts with '0b'.
            if (first == '0')
            {
                if (ch is 'x' or 'X')
                {
                    ++_index;
                    return ScanHexLiteral(start);
                }

                if (ch is 'b' or 'B')
                {
                    ++_index;
                    return ScanBinaryLiteral(start);
                }

                if (ch is 'o' or 'O')
                {
                    return ScanOctalLiteral(ch, start);
                }

                if (ch is '_')
                {
                    TolerateUnexpectedToken(Messages.NumericSeparatorAfterLeadingZero);
                }

                nonOctal = char.IsNumber(ch);
                if (nonOctal)
                {
                    if (strict)
                    {
                        ThrowUnexpectedToken(Messages.StrictDecimalWithLeadingZero);
                    }
                }

                if (ch > 0 && Character.IsOctalDigit(ch))
                {
                    if (IsImplicitOctalLiteral())
                    {
                        return ScanOctalLiteral(ch, start, true);
                    }
                }
            }

            --_index;
            sb.Append(this.ScanLiteralPart(Character.IsDecimalDigitFunc, allowNumericSeparator: !nonOctal, out _));
            ch = _source.CharCodeAt(_index);
        }

        if (ch == '.')
        {
            sb.Append(_source[_index++]);
            sb.Append(this.ScanLiteralPart(Character.IsDecimalDigitFunc, allowNumericSeparator: !nonOctal, out _));
            ch = _source.CharCodeAt(_index);
        }

        if (ch == 'e' || ch == 'E')
        {
            sb.Append(_source[_index++]);

            ch = _source.CharCodeAt(_index);
            if (ch == '+' || ch == '-')
            {
                sb.Append(_source[_index++]);
            }

            if (Character.IsDecimalDigit(_source.CharCodeAt(_index)))
            {
                sb.Append(this.ScanLiteralPart(Character.IsDecimalDigitFunc, allowNumericSeparator: true, out _));
            }
            else
            {
                ThrowUnexpectedToken();
            }
        }
        else if (ch == 'n')
        {
            if (nonOctal)
            {
                ThrowUnexpectedToken();
            }

            _index++;
            return ScanBigIntLiteral(start, sb.ToString().AsSpan(), JavaScriptNumberStyle.Integer);
        }

        if (Character.IsIdentifierStart(_source.CharCodeAt(_index)))
        {
            ThrowUnexpectedToken();
        }

        var number = sb.ToString();

        double value;
        if (long.TryParse(
            number,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out var l))
        {
            value = l;
        }
        else if (double.TryParse(
            number, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out var d))
        {
            value = d;
        }
        else
        {
            d = number.TrimStart().StartsWith("-", StringComparison.Ordinal)
                ? double.NegativeInfinity
                : double.PositiveInfinity;

            value = d;
        }

        return Token.CreateNumericLiteral(value, octal: false, start, end: _index, _lineNumber, _lineStart);
    }

    // https://tc39.github.io/ecma262/#sec-literals-string-literals

    private Token ScanStringLiteral(bool strict)
    {
        var start = _index;
        var quote = _source[start];
        //assert((quote == '\'' || quote == '"'),
        //    'String literal must starts with a quote');

        ++_index;
        var octal = false;
        var str = GetStringBuilder();

        while (!Eof())
        {
            var ch = _index < _source.Length ? _source[_index] : char.MinValue;
            _index++;

            if (ch == quote)
            {
                quote = char.MinValue;
                break;
            }
            else if (ch == '\\')
            {
                ch = _index < _source.Length ? _source[_index] : char.MinValue;
                _index++;
                if (ch == char.MinValue || !Character.IsLineTerminator(ch))
                {
                    switch (ch)
                    {
                        case 'u':
                            if (_index < _source.Length && _source[_index] == '{')
                            {
                                ++_index;
                                str.Append(ParserExtensions.CodePointOrSurrogateToString(ScanUnicodeCodePointEscape()));
                            }
                            else
                            {
                                if (!ScanHexEscape(ch, out var unescaped))
                                {
                                    ThrowUnexpectedToken();
                                }

                                str.Append(unescaped);
                            }

                            break;
                        case 'x':
                            if (!ScanHexEscape(ch, out var unescaped2))
                            {
                                ThrowUnexpectedToken(Messages.InvalidHexEscapeSequence);
                            }

                            str.Append(unescaped2);
                            break;
                        case 'n':
                            str.Append("\n");
                            break;
                        case 'r':
                            str.Append("\r");
                            break;
                        case 't':
                            str.Append("\t");
                            break;
                        case 'b':
                            str.Append("\b");
                            break;
                        case 'f':
                            str.Append("\f");
                            break;
                        case 'v':
                            str.Append("\x0B");
                            break;
                        case '8':
                        case '9':
                            str.Append(ch);
                            if (strict)
                            {
                                TolerateUnexpectedToken();
                            }
                            break;

                        default:
                            if (ch != char.MinValue && Character.IsOctalDigit(ch))
                            {
                                var octToDec = OctalToDecimal(ch);

                                if (octToDec.Octal)
                                {
                                    octal = true;
                                    if (strict)
                                    {
                                        TolerateUnexpectedToken(Messages.StrictOctalLiteral);
                                    }
                                }

                                str.Append((char) octToDec.Code);
                            }
                            else
                            {
                                str.Append(ch);
                            }

                            break;
                    }
                }
                else
                {
                    ++_lineNumber;
                    if (ch == '\r' && _source[_index] == '\n')
                    {
                        ++_index;
                    }

                    _lineStart = _index;
                }
            }
            else if (Character.IsLineTerminator(ch))
            {
                break;
            }
            else
            {
                str.Append(ch);
            }
        }

        if (quote != char.MinValue)
        {
            _index = start;
            ThrowUnexpectedToken();
        }

        return Token.CreateStringLiteral(str.ToString(), octal, start, end: _index, _lineNumber, _lineStart);
    }

    // https://tc39.github.io/ecma262/#sec-template-literal-lexical-components

    private Token ScanTemplate()
    {
        var cooked = GetStringBuilder();
        var terminated = false;
        var start = _index;

        var head = _source[start] == '`';
        var tail = false;
        char notEscapeSequenceHead = default;
        var rawOffset = 2;

        ++_index;

        while (!Eof())
        {
            var ch = _source[_index++];
            if (ch == '`')
            {
                rawOffset = 1;
                tail = true;
                terminated = true;
                break;
            }
            else if (ch == '$')
            {
                if (_source[_index] == '{')
                {
                    _curlyStack.Add("${");
                    ++_index;
                    terminated = true;
                    break;
                }

                cooked.Append(ch);
            }
            else if (notEscapeSequenceHead != default)
            {
                continue;
            }
            else if (ch == '\\')
            {
                ch = _source[_index++];
                if (!Character.IsLineTerminator(ch))
                {
                    switch (ch)
                    {
                        case 'n':
                            cooked.Append("\n");
                            break;
                        case 'r':
                            cooked.Append("\r");
                            break;
                        case 't':
                            cooked.Append("\t");
                            break;
                        case 'u':
                            if (_source[_index] == '{')
                            {
                                ++_index;
                                if (!TryScanUnicodeCodePointEscape(out var cp))
                                {
                                    notEscapeSequenceHead = cp > Character.UnicodeLastCodePoint ? 'v' : 'u';
                                }
                                else
                                {
                                    cooked.Append(ParserExtensions.CodePointOrSurrogateToString(cp));
                                }
                            }
                            else
                            {
                                if (!ScanHexEscape(ch, out var unescapedChar))
                                {
                                    notEscapeSequenceHead = 'u';
                                }
                                else
                                {
                                    cooked.Append(unescapedChar);
                                }
                            }

                            break;
                        case 'x':
                            if (!ScanHexEscape(ch, out var unescaped2))
                            {
                                notEscapeSequenceHead = 'x';
                            }
                            else
                            {
                                cooked.Append(unescaped2);
                            }

                            break;
                        case 'b':
                            cooked.Append("\b");
                            break;
                        case 'f':
                            cooked.Append("\f");
                            break;
                        case 'v':
                            cooked.Append("\v");
                            break;

                        default:
                            if (ch == '0')
                            {
                                if (Character.IsDecimalDigit(_source[_index]))
                                {
                                    // NotEscapeSequence: \01 \02 and so on
                                    notEscapeSequenceHead = '0';
                                }
                                else
                                {
                                    cooked.Append("\0");
                                }
                            }
                            else if (Character.IsDecimalDigit(ch))
                            {
                                // NotEscapeSequence: \1 \2
                                notEscapeSequenceHead = ch;
                            }
                            else
                            {
                                cooked.Append(ch);
                            }

                            break;
                    }
                }
                else
                {
                    ++_lineNumber;
                    if (ch == '\r' && _source[_index] == '\n')
                    {
                        ++_index;
                    }

                    _lineStart = _index;
                }
            }
            else if (Character.IsLineTerminator(ch))
            {
                ++_lineNumber;
                if (ch == '\r' && _source[_index] == '\n')
                {
                    ++_index;
                }

                _lineStart = _index;
                cooked.Append("\n");
            }
            else
            {
                cooked.Append(ch);
            }
        }

        if (!terminated)
        {
            ThrowUnexpectedToken();
        }

        if (!head)
        {
            _curlyStack.RemoveAt(_curlyStack.Count - 1);
        }

        var startRaw = start + 1;
        var endRaw = _index - rawOffset;
        var rawTemplate = _source.AsSpan(startRaw, endRaw - startRaw).ToInternedString(ref _stringPool, NonIdentifierInterningThreshold);
        var value = notEscapeSequenceHead == default ? cooked.ToString() : null;

        return Token.CreateTemplate(cooked: value, rawTemplate, head, tail, notEscapeSequenceHead, start, end: _index, _lineNumber, _lineStart);
    }

    /// <summary>
    /// Checks whether an ECMAScript regular expression is syntactically correct.
    /// </summary>
    /// <remarks>
    /// Expressions within Unicode property escape sequences (\p{...} and \P{...}) are not validated currently.
    /// </remarks>
    /// <returns><see langword="true"/> if the regular expression is syntactically correct, otherwise <see langword="false"/>.</returns>
    public static bool ValidateRegExp(string pattern, string flags, out ParseError? error)
    {
        if (pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern));
        }

        if (flags is null)
        {
            throw new ArgumentNullException(nameof(flags));
        }

        var scannerOptions = new ScannerOptions { RegExpParseMode = RegExpParseMode.Validate, Tolerant = false };

        try
        {
            new RegexParser(pattern, flags, scannerOptions).Parse(out _);
        }
        catch (ParserException ex)
        {
            error = ex.Error;
            return false;
        }

        error = default;
        return true;
    }

    /// <summary>
    /// Parses an ECMAScript regular expression and tries to construct a <see cref="Regex"/> instance with the equivalent behavior.
    /// </summary>
    /// <remarks>
    /// Please note that, because of some fundamental differences between the ECMAScript and .NET regular expression engines,
    /// not every ECMAScript regular expression can be converted to an equivalent <see cref="Regex"/> (or can be converted with compromises only).
    /// You can read more about the known issues of the conversion <see href="https://github.com/sebastienros/esprima-dotnet/pull/364#issuecomment-1606045259">here</see>.
    /// </remarks>
    /// <returns>
    /// The equivalent <see cref="Regex"/> if the conversion was possible,
    /// otherwise <see langword="null"/> (unless <paramref name="throwIfNotAdaptable"/> is <see langword="true"/>).
    /// </returns>
    /// <exception cref="ParserException">
    /// <paramref name="pattern"/> is an invalid regex pattern or cannot be converted
    /// to an equivalent <see cref="Regex"/> (if <paramref name="throwIfNotAdaptable"/> is <see langword="true"/>).
    /// </exception>
    public static Regex? AdaptRegExp(string pattern, string flags, bool compiled = false, TimeSpan? matchTimeout = null, bool throwIfNotAdaptable = false)
    {
        if (pattern is null)
        {
            throw new ArgumentNullException(nameof(pattern));
        }

        if (flags is null)
        {
            throw new ArgumentNullException(nameof(flags));
        }

        var defaultOptions = ScannerOptions.Default;
        matchTimeout ??= defaultOptions.RegexTimeout;
        var parseMode = !compiled ? RegExpParseMode.AdaptToInterpreted : RegExpParseMode.AdaptToCompiled;
        var tolerant = !throwIfNotAdaptable;

        var scannerOptions = parseMode == defaultOptions.RegExpParseMode && matchTimeout.Value == defaultOptions.RegexTimeout && tolerant == defaultOptions.Tolerant
            ? ScannerOptions.Default
            : new ScannerOptions { RegExpParseMode = parseMode, RegexTimeout = matchTimeout.Value, Tolerant = tolerant };

        return new RegexParser(pattern, flags, scannerOptions).Parse(out _);
    }

    private string ScanRegExpBody()
    {
        var ch = _source[_index];
        //assert(ch == '/', 'Regular expression literal must start with a slash');

        var str = GetStringBuilder();
        str.Append(_source[_index++]);
        var classMarker = false;
        var terminated = false;

        while (!Eof())
        {
            ch = _source[_index++];
            str.Append(ch);
            if (ch == '\\')
            {
                ch = _source[_index++];
                // https://tc39.github.io/ecma262/#sec-literals-regular-expression-literals
                if (Character.IsLineTerminator(ch))
                {
                    ThrowUnexpectedToken(Messages.UnterminatedRegExp);
                }

                str.Append(ch);
            }
            else if (Character.IsLineTerminator(ch))
            {
                ThrowUnexpectedToken(Messages.UnterminatedRegExp);
            }
            else if (classMarker)
            {
                if (ch == ']')
                {
                    classMarker = false;
                }
            }
            else
            {
                if (ch == '/')
                {
                    terminated = true;
                    break;
                }
                else if (ch == '[')
                {
                    classMarker = true;
                }
            }
        }

        if (!terminated)
        {
            ThrowUnexpectedToken(Messages.UnterminatedRegExp);
        }

        // Exclude leading and trailing slash.
        var body = str.ToString(1, str.Length - 2);
        return body;
    }

    private string ScanRegExpFlags()
    {
        var flags = "";
        while (!Eof())
        {
            var ch = _source[_index];
            if (!Character.IsIdentifierStart(ch))
            {
                break;
            }

            ++_index;
            if (ch == '\\' && !Eof())
            {
                ch = _source[_index];
                if (ch == 'u')
                {
                    ++_index;
                    var restore = _index;
                    if (ScanHexEscape('u', out ch))
                    {
                        flags += ch;
                    }
                    else
                    {
                        _index = restore;
                        flags += 'u';
                    }

                    TolerateUnexpectedToken();
                }
                else
                {
                    TolerateUnexpectedToken();
                }
            }
            else
            {
                flags += ch;
            }
        }

        return flags;
    }

    internal Token ScanRegExp()
    {
        var bodyStart = _index;
        var body = ScanRegExpBody();

        var flagsStart = _index;
        var flags = ScanRegExpFlags();

        var value = _regExpParseMode != RegExpParseMode.Skip
            ? new RegexParser(body, bodyStart + 1, flags, flagsStart, this).Parse(out _)
            : null;

        return Token.CreateRegexLiteral(value, new RegexValue(body, flags), bodyStart, end: _index, _lineNumber, _lineStart);
    }

    public Token Lex() => Lex(new LexOptions());

    internal Token Lex(in LexOptions options)
    {
        if (Eof())
        {
            ReleaseLargeBuffers();

            return Token.CreateEof(_index, _lineNumber, _lineStart);
        }

        var cp = _source[_index];

        // IsIdentifierStart also matches backslash and the surrogate range (U+D800..U+DFFF).
        if (Character.IsIdentifierStart(cp))
        {
            return ScanIdentifier(options.AllowIdentifierEscape);
        }

        // Very common: ( and ) and ;
        if (cp == 0x28 || cp == 0x29 || cp == 0x3B)
        {
            return ScanPunctuator();
        }

        // String literal starts with single quote (U+0027) or double quote (U+0022).
        if (cp == 0x27 || cp == 0x22)
        {
            return ScanStringLiteral(options.Strict);
        }

        // Dot (.) U+002E can also start a floating-point number, hence the need
        // to check the next character.
        if (cp == 0x2E)
        {
            if (Character.IsDecimalDigit(_source.CharCodeAt(_index + 1)))
            {
                return ScanNumericLiteral(options.Strict);
            }

            return ScanPunctuator();
        }

        if (Character.IsDecimalDigit(cp))
        {
            return ScanNumericLiteral(options.Strict);
        }

        // Template literals start with ` (U+0060) for template head
        // or } (U+007D) for template middle or template tail.
        if (cp == 0x60 || cp == 0x7D && _curlyStack.Count > 0 && _curlyStack[_curlyStack.Count - 1] == "${")
        {
            return ScanTemplate();
        }

        return ScanPunctuator();
    }

    internal Marker GetMarker() => new(_index, _lineNumber, Column: _index - _lineStart);
}

internal readonly record struct OctalValue(int Code, bool Octal);
