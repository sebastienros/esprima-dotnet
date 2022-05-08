using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Esprima.Ast;

namespace Esprima
{
    public sealed class SourceLocation
    {
        public Position? Start;
        public Position? End;
    }

    internal readonly struct ScannerState
    {
        public readonly int Index;
        public readonly int LineNumber;
        public readonly int LineStart;
        public readonly List<string> CurlyStack;

        public ScannerState(int index, int lineNumber, int lineStart, List<string> curlyStack)
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

    public sealed class Scanner
    {
        private readonly IErrorHandler _errorHandler;
        private readonly bool _trackComment;
        private readonly bool _adaptRegexp;
        private readonly TimeSpan _regexTimeout;
        private readonly int _length;

        public readonly string Source;
        public int Index;
        public int LineNumber;
        public int LineStart;

        internal bool IsModule;

        private List<string> _curlyStack;
        private readonly StringBuilder strb = new();

        private static readonly HashSet<string> Keywords = new()
        {
            "if",
            "in",
            "do",
            "var",
            "for",
            "new",
            "try",
            "let",
            "this",
            "else",
            "case",
            "void",
            "with",
            "enum",
            "while",
            "break",
            "catch",
            "throw",
            "const",
            "yield",
            "class",
            "super",
            "return",
            "typeof",
            "delete",
            "switch",
            "export",
            "import",
            "default",
            "finally",
            "extends",
            "function",
            "continue",
            "debugger",
            "instanceof"
        };

        private static readonly HashSet<string> StrictModeReservedWords = new()
        {
            "implements",
            "interface",
            "package",
            "private",
            "protected",
            "public",
            "static",
            "yield",
            "let"
        };

        private static readonly HashSet<string> FutureReservedWords = new() { "enum", "export", "import", "super" };

        private static readonly string[] threeCharacterPunctutors = { "===", "!==", ">>>", "<<=", ">>=", "**=", "&&=", "||=" };

        private static readonly string[] twoCharacterPunctuators = { "&&", "||", "==", "!=", "+=", "-=", "*=", "/=", "++", "--", "<<", ">>", "&=", "|=", "^=", "%=", "<=", ">=", "=>", "**" };

        private static int HexValue(char ch)
        {
            if (ch >= 'A')
            {
                if (ch >= 'a')
                {
                    if (ch <= 'h')
                    {
                        return ch - 'a' + 10;
                    }
                }
                else if (ch <= 'H')
                {
                    return ch - 'A' + 10;
                }
            }
            else if (ch <= '9')
            {
                return ch - '0';
            }

            return 0;
        }

        private static int OctalValue(char ch)
        {
            return ch - '0';
        }

        public Scanner(string code) : this(code, new ParserOptions())
        {
        }

        public Scanner(string code, ParserOptions options)
        {
            Source = code;
            _adaptRegexp = options.AdaptRegexp;
            _regexTimeout = options.RegexTimeout;
            _errorHandler = options.ErrorHandler;
            _trackComment = options.Comment;

            _length = code.Length;
            Index = 0;
            LineNumber = code.Length > 0 ? 1 : 0;
            LineStart = 0;
            _curlyStack = new List<string>(20);
        }


        internal ScannerState SaveState()
        {
            return new ScannerState(Index, LineNumber, LineStart, new List<string>(_curlyStack));
        }

        internal void RestoreState(in ScannerState state)
        {
            Index = state.Index;
            LineNumber = state.LineNumber;
            LineStart = state.LineStart;
            _curlyStack = state.CurlyStack;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Eof()
        {
            return Index >= _length;
        }

        private void ThrowUnexpectedToken(string message = Messages.UnexpectedTokenIllegal)
        {
            throw _errorHandler.CreateError(Index, LineNumber, Index - LineStart + 1, message);
        }

        private T ThrowUnexpectedToken<T>(string message = Messages.UnexpectedTokenIllegal)
        {
            throw _errorHandler.CreateError(Index, LineNumber, Index - LineStart + 1, message);
        }

        private void TolerateUnexpectedToken(string message = Messages.UnexpectedTokenIllegal)
        {
            _errorHandler.TolerateError(Index, LineNumber, Index - LineStart + 1, message);
        }

        private StringBuilder GetStringBuilder()
        {
            strb.Clear();
            return strb;
        }

        // https://tc39.github.io/ecma262/#sec-future-reserved-words

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFutureReservedWord(string? id)
        {
            return id != null && FutureReservedWords.Contains(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsStrictModeReservedWord(string? id)
        {
            return id != null && StrictModeReservedWords.Contains(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRestrictedWord(string? id)
        {
            return "eval".Equals(id) || "arguments".Equals(id);
        }

        // https://tc39.github.io/ecma262/#sec-keywords

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyword(string id)
        {
            return Keywords.Contains(id);
        }

        // https://tc39.github.io/ecma262/#sec-comments

        private ArrayList<Comment> SkipSingleLineComment(int offset)
        {
            var comments = new ArrayList<Comment>();
            var start = 0;
            var loc = new SourceLocation();

            if (_trackComment)
            {
                start = Index - offset;
                loc.Start = new Position(LineNumber, Index - LineStart - offset);
                loc.End = new Position();
            }

            while (!Eof())
            {
                var ch = Source.CharCodeAt(Index);
                ++Index;
                if (Character.IsLineTerminator(ch))
                {
                    if (_trackComment)
                    {
                        loc.End = new Position(LineNumber, Index - LineStart - 1);

                        var entry = new Comment
                        {
                            MultiLine = false,
                            Slice = new[] { start + offset, Index - 1 },
                            Start = start,
                            End = Index - 1,
                            Loc = loc
                        };

                        comments.Add(entry);
                    }

                    if (ch == 13 && Source.CharCodeAt(Index) == 10)
                    {
                        ++Index;
                    }

                    ++LineNumber;
                    LineStart = Index;
                    return comments;
                }
            }

            if (_trackComment)
            {
                loc.End = new Position(LineNumber, Index - LineStart);
                var entry = new Comment
                {
                    MultiLine = false,
                    Slice = new int[] { start + offset, Index },
                    Start = start,
                    End = Index,
                    Loc = loc
                };

                comments.Add(entry);
            }

            return comments;
        }

        private ArrayList<Comment> SkipMultiLineComment()
        {
            var comments = new ArrayList<Comment>();
            var start = 0;
            var loc = new SourceLocation();

            if (_trackComment)
            {
                start = Index - 2;
                loc.Start = new Position(LineNumber, Index - LineStart - 2);
            }

            while (!Eof())
            {
                var ch = Source.CharCodeAt(Index);
                if (Character.IsLineTerminator(ch))
                {
                    if (ch == 0x0D && Source.CharCodeAt(Index + 1) == 0x0A)
                    {
                        ++Index;
                    }

                    ++LineNumber;
                    ++Index;
                    LineStart = Index;
                }
                else if (ch == 0x2A)
                {
                    // Block comment ends with '*/'.
                    if (Source.CharCodeAt(Index + 1) == 0x2F)
                    {
                        Index += 2;
                        if (_trackComment)
                        {
                            loc.End = new Position(LineNumber, Index - LineStart);
                            var entry = new Comment
                            {
                                MultiLine = true,
                                Slice = new int[] { start + 2, Index - 2 },
                                Start = start,
                                End = Index,
                                Loc = loc
                            };
                            comments.Add(entry);
                        }

                        return comments;
                    }

                    ++Index;
                }
                else
                {
                    ++Index;
                }
            }

            // Ran off the end of the file - the whole thing is a comment
            if (_trackComment)
            {
                loc.End = new Position(LineNumber, Index - LineStart);
                var entry = new Comment
                {
                    MultiLine = true,
                    Slice = new int[] { start + 2, Index },
                    Start = start,
                    End = Index,
                    Loc = loc
                };
                comments.Add(entry);
            }

            TolerateUnexpectedToken();
            return comments;
        }

        public IReadOnlyList<Comment> ScanComments()
        {
            return ScanCommentsInternal();
        }

        internal ArrayList<Comment> ScanCommentsInternal()
        {
            var comments = new ArrayList<Comment>();

            var start = Index == 0;
            while (!Eof())
            {
                var ch = Source.CharCodeAt(Index);

                if (Character.IsWhiteSpace(ch))
                {
                    ++Index;
                }
                else if (Character.IsLineTerminator(ch))
                {
                    ++Index;
                    if (ch == 0x0D && Source.CharCodeAt(Index) == 0x0A)
                    {
                        ++Index;
                    }

                    ++LineNumber;
                    LineStart = Index;
                    start = true;
                }
                else if (ch == 0x2F)
                {
                    // U+002F is '/'
                    ch = Source.CharCodeAt(Index + 1);
                    if (ch == 0x2F)
                    {
                        Index += 2;
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
                        Index += 2;
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
                    if (Source.CharCodeAt(Index + 1) == 0x2D && Source.CharCodeAt(Index + 2) == 0x3E)
                    {
                        // '-->' is a single-line comment
                        Index += 3;
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
                    if (Source[Index + 1] == '!'
                        && Source[Index + 2] == '-'
                        && Source[Index + 3] == '-')
                    {
                        if (IsModule)
                        {
                            ThrowUnexpectedToken();
                        }

                        Index += 4; // `<!--`
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
                else
                {
                    break;
                }
            }

            return comments;
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

        public bool ScanHexEscape(char prefix, out char result)
        {
            var len = prefix == 'u' ? 4 : 2;
            var code = 0;

            for (var i = 0; i < len; ++i)
            {
                if (!Eof())
                {
                    var d = Source[Index];
                    if (Character.IsHexDigit(d))
                    {
                        code = code * 16 + HexValue(d);
                        Index++;
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

        public string? TryToScanUnicodeCodePointEscape()
        {
            var ch = Source[Index];
            var code = 0;

            // At least, one hex digit is required.
            if (ch == '}')
            {
                return null;
            }

            while (!Eof())
            {
                ch = Source[Index++];
                if (!Character.IsHexDigit(ch))
                {
                    break;
                }

                code = code * 16 + HexValue(ch);
            }

            if (code > 0x10FFFF || ch != '}')
            {
                return null;
            }

            return Character.FromCodePoint(code);
        }

        private string ScanUnicodeCodePointEscape()
        {
            var result = TryToScanUnicodeCodePointEscape();
            if (result is null)
            {
                ThrowUnexpectedToken();
            }

            return result!;
        }

        public string GetIdentifier()
        {
            var start = Index++;
            while (!Eof())
            {
                var ch = Source.CharCodeAt(Index);
                if (ch == 0x5C)
                {
                    // Blackslash (U+005C) marks Unicode escape sequence.
                    Index = start;
                    return GetComplexIdentifier();
                }
                else if (ch >= 0xD800 && ch < 0xDFFF)
                {
                    // Need to handle surrogate pairs.
                    Index = start;
                    return GetComplexIdentifier();
                }

                if (Character.IsIdentifierPart(ch))
                {
                    ++Index;
                }
                else
                {
                    break;
                }
            }

            return Source.Slice(start, Index);
        }

        public string GetComplexIdentifier()
        {
            var cp = CodePointAt(Source, Index);
            var id = Character.FromCodePoint(cp);
            Index += id.Length;

            // '\u' (U+005C, U+0075) denotes an escaped character.
            string ch;
            if (cp == 0x5C)
            {
                if (Source.CharCodeAt(Index) != 0x75)
                {
                    ThrowUnexpectedToken();
                }

                ++Index;
                if (Source[Index] == '{')
                {
                    ++Index;
                    ch = ScanUnicodeCodePointEscape();
                }
                else
                {
                    char ch1;
                    if (!ScanHexEscape('u', out ch1) || ch1 == '\\' || !Character.IsIdentifierStart(ch1))
                    {
                        ThrowUnexpectedToken();
                    }

                    ch = ParserExtensions.CharToString(ch1);
                }

                id = ch;
            }

            while (!Eof())
            {
                cp = CodePointAt(Source, Index);
                ch = Character.FromCodePoint(cp);
                if (!Character.IsIdentifierPart(ch))
                {
                    break;
                }

                id += ch;
                Index += ch.Length;

                // '\u' (U+005C, U+0075) denotes an escaped character.
                if (cp == 0x5C)
                {
                    id = id.Substring(0, id.Length - 1);
                    if (Source.CharCodeAt(Index) != 0x75)
                    {
                        ThrowUnexpectedToken();
                    }

                    ++Index;
                    if (Index < Source.Length && Source[Index] == '{')
                    {
                        ++Index;
                        ch = ScanUnicodeCodePointEscape();
                    }
                    else
                    {
                        if (!ScanHexEscape('u', out var ch1) || ch1 == '\\' || !Character.IsIdentifierPart(ch1))
                        {
                            ThrowUnexpectedToken();
                        }

                        ch = ParserExtensions.CharToString(ch1);
                    }

                    id += ch;
                }
            }

            return id;
        }

        public OctalValue OctalToDecimal(char ch)
        {
            // \0 is not octal escape sequence
            var octal = ch != '0';
            var code = OctalValue(ch);

            if (!Eof() && Character.IsOctalDigit(Source.CharCodeAt(Index)))
            {
                octal = true;
                code = code * 8 + OctalValue(Source[Index++]);

                // 3 digits are only allowed when string starts
                // with 0, 1, 2, 3
                if (ch >= '0' && ch <= '3' && !Eof() && Character.IsOctalDigit(Source.CharCodeAt(Index)))
                {
                    code = code * 8 + OctalValue(Source[Index++]);
                }
            }

            return new OctalValue(code, octal);
        }

        // https://tc39.github.io/ecma262/#sec-names-and-keywords

        private Token ScanIdentifier(bool allowEscapes)
        {
            TokenType type;
            var start = Index;

            // Backslash (U+005C) starts an escaped character.
            var id = Source.CharCodeAt(start) == 0x5C ? GetComplexIdentifier() : GetIdentifier();

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

            if (type != TokenType.Identifier && start + id.Length != Index)
            {
                var restore = Index;
                Index = start;
                if (!allowEscapes)
                {
                    TolerateUnexpectedToken(Messages.InvalidEscapedReservedWord);
                }
                Index = restore;
            }

            return new Token
            {
                Type = type,
                Value = id,
                LineNumber = LineNumber,
                LineStart = LineStart,
                Start = start,
                End = Index
            };
        }

        // https://tc39.github.io/ecma262/#sec-punctuators

        public Token ScanPunctuator()
        {
            static string SafeSubstring(string s, int startIndex, int length)
            {
                return startIndex + length > s.Length ? string.Empty : s.Substring(startIndex, length);
            }

            var start = Index;

            // Check for most common single-character punctuators.
            // TODO spanify
            var c = Source[Index];
            var str = c.ToString();

            switch (c)
            {
                case '(':
                case '{':
                    if (c == '{')
                    {
                        _curlyStack.Add("{");
                    }

                    ++Index;
                    break;

                case '.':
                    ++Index;
                    if (Source.Length >= Index + 2 && Source[Index] == '.' && Source[Index + 1] == '.')
                    {
                        // Spread operator: ...
                        Index += 2;
                        str = "...";
                    }

                    break;

                case '}':
                    ++Index;
                    if (_curlyStack.Count > 0)
                    {
                        _curlyStack.RemoveAt(_curlyStack.Count - 1);
                    }

                    break;


                case '?':
                    ++Index;
                    if (Source[Index] == '?')
                    {
                        ++Index;
                        if (Source[Index] == '=')
                        {
                            ++Index;
                            str = "??=";
                        }
                        else
                        {
                            str = "??";
                        }
                    }

                    if (Source[Index] == '.' && !char.IsDigit(Source[Index + 1]))
                    {
                        // "?." in "foo?.3:0" should not be treated as optional chaining.
                        // See https://github.com/tc39/proposal-optional-chaining#notes
                        ++Index;
                        str = "?.";
                    }

                    break;

                case ')':
                case ';':
                case ',':
                case '[':
                case ']':
                case ':':
                case '~':
                    ++Index;
                    break;

                default:

                    // 4-character punctuator.
                    str = SafeSubstring(Source, Index, 4);
                    if (str == ">>>=")
                    {
                        Index += 4;
                    }
                    else
                    {
                        // 3-character punctuators.
                        str = SafeSubstring(Source, Index, 3);
                        if (str.Length == 3 && FindThreeCharEqual(str, threeCharacterPunctutors) != null)
                        {
                            Index += 3;
                        }
                        else
                        {
                            // 2-character punctuators.
                            str = SafeSubstring(Source, Index, 2);
                            if (str.Length == 2 && FindTwoCharEqual(str, twoCharacterPunctuators) != null)
                            {
                                Index += 2;
                            }
                            else
                            {
                                // 1-character punctuators.
                                str = Source[Index].ToString();
                                if ("<>=!+-*%&|^/".IndexOf(str, StringComparison.Ordinal) >= 0)
                                {
                                    ++Index;
                                }
                            }
                        }
                    }

                    break;
            }

            if (Index == start)
            {
                ThrowUnexpectedToken();
            }

            return new Token
            {
                Type = TokenType.Punctuator,
                Value = str,
                LineNumber = LineNumber,
                LineStart = LineStart,
                Start = start,
                End = Index
            };
        }

        // https://tc39.github.io/ecma262/#sec-literals-numeric-literals

        public Token ScanHexLiteral(int start)
        {
            var number = this.ScanLiteralPart(Character.IsHexDigitFunc, allowNumericSeparator: true, out var hasUpperCase);

            if (number.Length == 0)
            {
                ThrowUnexpectedToken();
            }

            var ch = Source.CharCodeAt(Index);
            if (Character.IsIdentifierStart(ch))
            {
                if (ch == 'n')
                {
                    Index++;
                    return ScanBigIntLiteral(start, number, JavascriptNumberStyle.Hex);
                }
                ThrowUnexpectedToken();
            }

            double value = 0;

            if (number.Length < 16)
            {
#if !HAS_SPAN_PARSE
                value = Convert.ToInt64(number.ToString(), 16);
#else
                value = long.Parse(number, NumberStyles.HexNumber, null);
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

            return new Token
            {
                Type = TokenType.NumericLiteral,
                NumericValue = value,
                Value = value,
                LineNumber = LineNumber,
                LineStart = LineStart,
                Start = start,
                End = Index
            };
        }

        private enum JavascriptNumberStyle
        {
            Binary,
            Hex,
            Octal,
            Integer
        }

        private Token ScanBigIntLiteral(int start, ReadOnlySpan<char> number, JavascriptNumberStyle style)
        {
            BigInteger bigInt = 0;
            if (style == JavascriptNumberStyle.Binary)
            {
                // binary
                foreach (var c in number)
                {
                    bigInt <<= 1;
                    bigInt += c == '1' ? 1 : 0;
                }
            }
            else
            {
                if (style == JavascriptNumberStyle.Hex)
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
                    JavascriptNumberStyle.Integer => NumberStyles.Integer,
                    JavascriptNumberStyle.Hex => NumberStyles.HexNumber,
                    _ => NumberStyles.None
                };
#if HAS_SPAN_PARSE
                bigInt = BigInteger.Parse(number, parseStyle);
#else
                bigInt = BigInteger.Parse(number.ToString(), parseStyle);
#endif
            }

            return new Token
            {
                Type = TokenType.BigIntLiteral,
                Value = bigInt,
                BigIntValue = bigInt,
                LineNumber = LineNumber,
                LineStart = LineStart,
                Start = start,
                End = Index
            };
        }

        public Token ScanBinaryLiteral(int start)
        {
            var number = this.ScanLiteralPart(static c => c is '0' or '1', allowNumericSeparator: true, out _);

            if (number.Length == 0)
            {
                // only 0b or 0B
                ThrowUnexpectedToken();
            }

            if (!Eof())
            {
                var ch = Source.CharCodeAt(Index);
                if (ch == 'n')
                {
                    Index++;
                    return ScanBigIntLiteral(start, number, JavascriptNumberStyle.Binary);
                }

                if (Character.IsIdentifierStart(ch) || Character.IsDecimalDigit(ch))
                {
                    ThrowUnexpectedToken();
                }
            }

            var numberString = number.ToString();
            var token = new Token
            {
                Type = TokenType.NumericLiteral,
                NumericValue = Convert.ToUInt32(numberString, 2),
                Value = numberString,
                LineNumber = LineNumber,
                LineStart = LineStart,
                Start = start,
                End = Index
            };

            return token;
        }

        public Token ScanOctalLiteral(char prefix, int start, bool isLegacyOctalDigital = false)
        {
            var sb = GetStringBuilder();
            var octal = false;

            if (Character.IsOctalDigit(prefix))
            {
                octal = true;
                sb.Append("0").Append(Source[Index++]);
            }
            else
            {
                ++Index;
            }

            sb.Append(this.ScanLiteralPart(Character.IsOctalDigitFunc, allowNumericSeparator: true, out _));
            var number = sb.ToString();

            if (!octal && number.Length == 0)
            {
                // only 0o or 0O
                ThrowUnexpectedToken();
            }

            var ch = Source.CharCodeAt(Index);
            
            if (ch == 'n')
            {
                if (isLegacyOctalDigital)
                {
                    ThrowUnexpectedToken();
                }
           
                Index++;
                return ScanBigIntLiteral(start, number.AsSpan(), JavascriptNumberStyle.Octal);
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

            return new Token
            {
                Type = TokenType.NumericLiteral,
                NumericValue = numericValue,
                Value = number,
                Octal = octal,
                LineNumber = LineNumber,
                LineStart = LineStart,
                Start = start,
                End = Index
            };
        }

        public bool IsImplicitOctalLiteral()
        {
            // Implicit octal, unless there is a non-octal digit.
            // (Annex B.1.1 on Numeric Literals)
            for (var i = Index + 1; i < _length; ++i)
            {
                var ch = Source[i];
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
            var start = Index;

            var charCode = Source.CharCodeAt(Index);
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

                Index++;
                var newCharCode = Source.CharCodeAt(Index);
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

            if (Source[Index -1] == '_')
            {
                ThrowUnexpectedToken(Messages.NumericSeparatorNotAllowedHere);
            }

            var span = Source.AsSpan(start, Index - start);
            return needsCleanup
                ? span.ToString().Replace("_", "").AsSpan()
                : span;
        }

        public Token ScanNumericLiteral(bool strict)
        {
            var sb = GetStringBuilder();
            var start = Index;
            var ch = Source[start];
            //assert(Character.IsDecimalDigit(ch) || (ch == '.'),
            //    'Numeric literal must start with a decimal digit or a decimal point');

            var nonOctal = false;
            if (ch != '.')
            {
                var first = Source[Index++];
                ch = Source.CharCodeAt(Index);

                // Hex number starts with '0x'.
                // Octal number starts with '0'.
                // Octal number in ES6 starts with '0o'.
                // Binary number in ES6 starts with '0b'.
                if (first == '0')
                {
                    if (ch is 'x' or 'X')
                    {
                        ++Index;
                        return ScanHexLiteral(start);
                    }

                    if (ch is 'b' or 'B')
                    {
                        ++Index;
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
                    if (nonOctal && strict)
                    {
                        TolerateUnexpectedToken(Messages.StrictDecimalWithLeadingZero);
                    }

                    if (ch > 0 && Character.IsOctalDigit(ch))
                    {
                        if (IsImplicitOctalLiteral())
                        {
                            return ScanOctalLiteral(ch, start, true);
                        }
                    }
                }

                --Index;
                sb.Append(this.ScanLiteralPart(Character.IsDecimalDigitFunc, allowNumericSeparator: !nonOctal, out _));
                ch = Source.CharCodeAt(Index);
            }

            if (ch == '.')
            {
                sb.Append(Source[Index++]);
                sb.Append(this.ScanLiteralPart(Character.IsDecimalDigitFunc, allowNumericSeparator: !nonOctal, out _));
                ch = Source.CharCodeAt(Index);
            }

            if (ch == 'e' || ch == 'E')
            {
                sb.Append(Source[Index++]);

                ch = Source.CharCodeAt(Index);
                if (ch == '+' || ch == '-')
                {
                    sb.Append(Source[Index++]);
                }

                if (Character.IsDecimalDigit(Source.CharCodeAt(Index)))
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

                Index++;
                return ScanBigIntLiteral(start, sb.ToString().AsSpan(), JavascriptNumberStyle.Integer);
            }

            if (Character.IsIdentifierStart(Source.CharCodeAt(Index)))
            {
                ThrowUnexpectedToken();
            }

            var token = new Token
            {
                Type = TokenType.NumericLiteral,
                LineNumber = LineNumber,
                LineStart = LineStart,
                Start = start,
                End = Index
            };

            var number = sb.ToString();

            if (long.TryParse(
                number,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out var l))
            {
                token.NumericValue = l;
                token.Value = l;
            }
            else if (double.TryParse(
                number, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out var d))
            {
                token.NumericValue = d;
                token.Value = d;
            }
            else
            {
                d = number.TrimStart().StartsWith("-")
                    ? double.NegativeInfinity
                    : double.PositiveInfinity;

                token.NumericValue = d;
                token.Value = d;
            }

            return token;
        }

        // https://tc39.github.io/ecma262/#sec-literals-string-literals

        public Token ScanStringLiteral(bool strict)
        {
            var start = Index;
            var quote = Source[start];
            //assert((quote == '\'' || quote == '"'),
            //    'String literal must starts with a quote');

            ++Index;
            var octal = false;
            var str = GetStringBuilder();

            while (!Eof())
            {
                var ch = Index < Source.Length ? Source[Index] : char.MinValue;
                Index++;

                if (ch == quote)
                {
                    quote = char.MinValue;
                    break;
                }
                else if (ch == '\\')
                {
                    ch = Index < Source.Length ? Source[Index] : char.MinValue;
                    Index++;
                    if (ch == char.MinValue || !Character.IsLineTerminator(ch))
                    {
                        switch (ch)
                        {
                            case 'u':
                                if (Index < Source.Length && Source[Index] == '{')
                                {
                                    ++Index;
                                    str.Append(ScanUnicodeCodePointEscape());
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
                        ++LineNumber;
                        if (ch == '\r' && Source[Index] == '\n')
                        {
                            ++Index;
                        }

                        LineStart = Index;
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
                Index = start;
                ThrowUnexpectedToken();
            }

            return new Token
            {
                Type = TokenType.StringLiteral,
                Value = str.ToString(),
                Octal = octal,
                LineNumber = LineNumber,
                LineStart = LineStart,
                Start = start,
                End = Index
            };
        }

        // https://tc39.github.io/ecma262/#sec-template-literal-lexical-components

        public Token ScanTemplate()
        {
            var cooked = GetStringBuilder();
            var terminated = false;
            var start = Index;

            var head = Source[start] == '`';
            var tail = false;
            char? notEscapeSequenceHead = null;
            var rawOffset = 2;

            ++Index;

            while (!Eof())
            {
                var ch = Source[Index++];
                if (ch == '`')
                {
                    rawOffset = 1;
                    tail = true;
                    terminated = true;
                    break;
                }
                else if (ch == '$')
                {
                    if (Source[Index] == '{')
                    {
                        _curlyStack.Add("${");
                        ++Index;
                        terminated = true;
                        break;
                    }

                    cooked.Append(ch);
                }
                else if (notEscapeSequenceHead is not null)
                {
                    continue;
                }
                else if (ch == '\\')
                {
                    ch = Source[Index++];
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
                                if (Source[Index] == '{')
                                {
                                    ++Index;
                                    var unicodeCodePointEscape = TryToScanUnicodeCodePointEscape();
                                    if (unicodeCodePointEscape is null)
                                    {
                                        notEscapeSequenceHead = 'u';
                                    }
                                    else
                                    {
                                        cooked.Append(unicodeCodePointEscape);
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
                                    if (Character.IsDecimalDigit(Source.CharCodeAt(Index)))
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
                        ++LineNumber;
                        if (ch == '\r' && Source[Index] == '\n')
                        {
                            ++Index;
                        }

                        LineStart = Index;
                    }
                }
                else if (Character.IsLineTerminator(ch))
                {
                    ++LineNumber;
                    if (ch == '\r' && Source[Index] == '\n')
                    {
                        ++Index;
                    }

                    LineStart = Index;
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

            return new Token
            {
                Type = TokenType.Template,
                Value = notEscapeSequenceHead is null ? cooked.ToString() : null,
                RawTemplate = Source.Slice(start + 1, Index - rawOffset),
                Head = head,
                Tail = tail,
                NotEscapeSequenceHead = notEscapeSequenceHead,
                LineNumber = LineNumber,
                LineStart = LineStart,
                Start = start,
                End = Index
            };
        }

        private static string FromCharCode(uint[] codeUnits)
        {
            var chars = new char[codeUnits.Length];
            for (var i = 0; i < chars.Length; i++)
            {
                chars[i] = (char) codeUnits[i];
            }

            return new string(chars);
        }

        private string FromCodePoint(params uint[] codePoints)
        {
            var codeUnits = new List<uint>();
            var result = "";

            foreach (var codePoint in codePoints)
            {
                if (codePoint < 0 || codePoint > 0x10FFFF)
                {
                    EsprimaExceptionHelper.ThrowArgumentOutOfRangeException(nameof(codePoint), codePoint, "Invalid code point.");
                }

                var point = codePoint;
                if (point <= 0xFFFF)
                {
                    // BMP code point
                    codeUnits.Add(point);
                }
                else
                {
                    // Astral code point; split in surrogate halves
                    // https://mathiasbynens.be/notes/javascript-encoding#surrogate-formulae
                    point -= 0x10000;
                    codeUnits.Add((point >> 10) + 0xD800); // highSurrogate
                    codeUnits.Add((point % 0x400) + 0xDC00); // lowSurrogate
                }
                if (codeUnits.Count >= 0x3fff)
                {
                    result += FromCharCode(codeUnits.ToArray());
                    codeUnits.Clear();
                }
            }

            return result + FromCharCode(codeUnits.ToArray());
        }

        /// <summary>
        /// Converts an ECMAScript regular expression to a <see cref="Regex"/> instance and tries to adapt regex
        /// to work in .NET when possible.
        /// </summary>
        public Regex ParseRegex(string pattern, string flags, TimeSpan matchTimeout)
        {
            var tmp = pattern;

            var isUnicode = flags.IndexOf('u') >= 0;

            CheckBracesBalance(pattern, isUnicode);

            tmp = Regex
                .Replace(tmp, @"(\\u[a-fA-F0-9]{4})+", (match) =>
                {
                    // e.g., \uD83D\uDE80 (which is equivalent to \u{1F680}
                    var codePoints = new uint[match.Value.Length / 6];

                    for (var i = 0; i < codePoints.Length; i++)
                    {
                        codePoints[i] = Convert.ToUInt32(match.Value.Substring(i * 6 + 2, 4), 16);
                    }

                    var sub = FromCodePoint(codePoints);

                    return sub;
                });

            if (isUnicode)
            {
                if (Regex.IsMatch(tmp, @"\\0[0-9]+"))
                {
                    throw new ParserException("Invalid decimal escape");
                }

                if (Regex.IsMatch(tmp, @"\\[1-9]\d*"))
                {
                    throw new ParserException("Invalid escape");
                }

                tmp = Regex
                    // Replace every Unicode escape sequence with the equivalent
                    // BMP character or a constant ASCII code point in the case of
                    // astral symbols. (See the above note on `astralSubstitute`
                    // for more information.)
                    .Replace(tmp, @"\\u\{([0-9a-fA-F]+)\}", (match) =>
                    {
                        var codePoint = Convert.ToUInt32(match.Groups[1].Value, 16);

                        if (codePoint > 0x10FFFF)
                        {
                            ThrowUnexpectedToken(Messages.InvalidRegExp);
                        }

                        return FromCodePoint(codePoint);
                    });

                tmp = ConvertUnicodeRegexRanges(tmp);
            }

            // \u is a valid escape sequence in JS, but not in .NET
            // search for any of these that are not valid \uxxxx values

            tmp = Regex.Replace(tmp, @"(\\+)u(?![a-fA-F0-9]{4})", static match =>
            {
                return new string('\\', match.Groups[1].Value.Length / 2 * 2) + 'u';
            });

            // First, detect invalid regular expressions.
            var options = ParseRegexOptions(flags);

            try
            {
                new Regex(tmp, options, matchTimeout);
            }
            catch
            {
                tmp = EscapeFailingRegex(tmp);

                try
                {
                    new Regex(tmp, options, matchTimeout);
                }
                catch (Exception ex)
                {
                    ThrowUnexpectedToken($"{Messages.InvalidRegExp}: {ex.Message}");
                }
            }

            // Replace all non-escaped $ occurences by \r?$
            // c.f. http://programmaticallyspeaking.com/regular-expression-multiline-mode-whats-a-newline.html

            var index = 0;
            var newPattern = tmp;

            if (options.HasFlag(RegexOptions.Multiline))
            {
                while ((index = newPattern.IndexOf("$", index, StringComparison.Ordinal)) != -1)
                {
                    if (index > 0 && newPattern[index - 1] != '\\')
                    {
                        newPattern = newPattern.Substring(0, index) + @"\r?" + newPattern.Substring(index);
                        index += 4;
                    }
                    else
                    {
                        index++;
                    }
                }
            }

            pattern = newPattern;

            return new Regex(pattern, options, matchTimeout);
        }

        /// <summary>
        /// Ensures the braces are balanced in a unicode Regex
        /// </summary>
        private void CheckBracesBalance(string pattern, bool unicode)
        {
            var inGroup = 0;
            var inQuantifier = false;
            var inSet = false;

            for (var i = 0; i < pattern.Length; i++)
            {
                var ch = pattern[i];

                if (ch == '\\')
                {
                    // Skip escape

                    i++;
                    continue;
                }

                switch (ch)
                {
                    case '(':

                        if (inSet)
                        {
                            break;
                        }

                        inGroup++;

                        break;

                    case ')':

                        if (inSet)
                        {
                            break;
                        }

                        if (inGroup == 0)
                        {
                            throw new ParserException(Messages.RegexUnmatchedOpenParen);
                        }

                        inGroup--;

                        break;

                    case '{':

                        if (inSet)
                        {
                            break;
                        }

                        if (!inQuantifier)
                        {
                            inQuantifier = true;
                        }
                        else if (unicode)
                        {
                            throw new ParserException(Messages.RegexIncompleteQuantifier);
                        }

                        break;

                    case '}':

                        if (inSet)
                        {
                            break;
                        }

                        if (inQuantifier)
                        {
                            inQuantifier = false;
                        }
                        else if (unicode)
                        {
                            throw new ParserException(Messages.RegexLoneQuantifierBrackets);
                        }

                        break;

                    case '[':

                        if (inSet)
                        {
                            break;
                        }

                        inSet = true;

                        break;

                    case ']':

                        if (inSet)
                        {
                            inSet = false;
                        }
                        else if (unicode)
                        {
                            throw new ParserException(Messages.RegexLoneQuantifierBrackets);
                        }

                        break;

                    default: break;
                }
            }

            if (inGroup > 0)
            {
                throw new ParserException(Messages.RegexUnterminatedGroup);
            }

            if (inSet)
            {
                throw new ParserException(Messages.RegexUnterminatedCharacterClass);
            }

            if (unicode)
            {
                if (inQuantifier)
                {
                    throw new ParserException(Messages.RegexLoneQuantifierBrackets);
                }
            }
        }

        private string ConvertUnicodeRegexRanges(string pattern)
        {
            if (String.IsNullOrEmpty(pattern))
            {
                return pattern;
            }

            bool converted = false;

            var sb = GetStringBuilder();

            for (var i = 0; i < pattern.Length; i++)
            {
                var ch = pattern[i];

                // Sets have to be converted char by char
                if (ch == '[' && i + 1 < pattern.Length)
                {
                    var inverted = pattern[i + 1] == '^';

                    if (inverted)
                    {
                        i++;
                    }

                    var next = i + 1;

                    while (next < pattern.Length && pattern[next] != ']')
                    {
                        // Consume escaped chars
                        if (pattern[next] == '\\')
                        {
                            next++;
                        }

                        next++;
                    }

                    // Reached end of pattern
                    if (next >= pattern.Length)
                    {
                        sb.Append('[');
                        continue;
                    }

                    var set = pattern.Substring(i + 1, next - i - 1);

                    // Convert the set of chars into their unicode

                    AppendConvertUnicodeSet(sb, set, inverted);

                    i = next;

                    converted = true;
                }
                else if (ch == '.')
                {
                    converted = true;

                    sb.Append("(?:[\uD800-\uDBFF][\uDC00-\uDFFF]|.)");
                }
                else if (ch == '\\' && i + 1 < pattern.Length)
                {
                    ch = pattern[++i];

                    if (ch == 'D' || ch == 'S' || ch == 'W')
                    {
                        converted = true;

                        sb.Append("(?:[\uD800-\uDBFF][\uDC00-\uDFFF]|\\").Append(ch).Append(')');
                    }
                    else
                    {
                        converted = true;

                        sb.Append('\\').Append(ch);
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }

            return converted ? sb.ToString() : pattern;
        }

        /// <summary>
        /// Converts a range ([..]) with a unicode flag to a compatible one for RegEx.
        /// </summary>
        internal static void AppendConvertUnicodeSet(StringBuilder sb, string set, bool inverted)
        {
            // \u{1F4A9} == 💩 == \ud83d\udca9
            // \u{1F4AB} == 💫 == \ud83d\udcab

            // Regex only looks at single System.Char units. U+1F4A9 for example is two Chars that, from Regex 's perspective, are independent.
            // "[💩-💫]" is "[\ud83d\udca9-\ud83d\udcab]", so it just looks at the individual Char values, it sees "\udca9-\ud83d", which is not ordered, hence the error.
            // This is a known design / limitation of Regex that's existed since it was added, and there are currently no plans to improve that.
            // The Regex needs to be rewritten to (?:\ud83d[\udca9-\udcab])

            // Each char or special notation (\s, \d, ...) is converted to a char range such they can be sorted, groupped or inverted.

            if (String.IsNullOrEmpty(set))
            {
                sb.Append("[]");
                return;
            }

            var ranges = CreateRanges(set);

            if (inverted)
            {
                InvertRanges(ranges);
            }

            var singleCharRanges = new List<Range>(ranges.Length);

            foreach (var range in ranges)
            {
                if (range.Start > range.End)
                {
                    throw new ParserException("Invalid regular expression: Range out of order in character class");
                }

                // If Start is not single char, End can't be either (greater), skip
                if (range.Start >= 0x10000)
                {
                    continue;
                }
                else if (range.End >= 0x10000)
                {
                    singleCharRanges.Add(new Range(range.Start, 0xFFFF));
                }
                else
                {
                    singleCharRanges.Add(range);
                }

                break;
            }

            var multiCharRanges = new List<Range>(ranges.Length - singleCharRanges.Count + 1);

            for (var i = 0; i < ranges.Length; i++)
            {
                if (ranges[i].Start >= 0x10000)
                {
                    multiCharRanges.Add(ranges[i]);
                }
                else if (ranges[i].End >= 0x10000)
                {
                    multiCharRanges.Add(new Range(0x10000, ranges[i].End));
                }
            }

            sb.Append("(?:");

            if (multiCharRanges.Count > 0)
            {
                sb.Append("(?:");

                for (var i = 0; i < multiCharRanges.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append('|');
                    }

                    var c = multiCharRanges[i];

                    if (c.Start == c.End)
                    {
                        sb.Append(char.ConvertFromUtf32(c.Start));
                    }
                    else
                    {
                        var start = char.ConvertFromUtf32(c.Start);
                        var stop = char.ConvertFromUtf32(c.End);

                        if (start[0] == stop[0])
                        {
                            sb.Append(start[0]).Append('[').Append(start[1]).Append('-').Append(stop[1]).Append(']');
                        }
                        else
                        {
                            var s1 = (start[1] > 0xDC00) ? 1 : 0;
                            var s2 = (stop[1] < 0xDFFF) ? 1 : 0;

                            if (s1 != 0)
                            {
                                sb.Append(start[0]).Append('[').Append(start[1]).Append("-\uDFFF]|");
                            }

                            if (stop[0] - start[0] >= s1 + s2)
                            {
                                sb.Append('[');
                                sb.Append((char) (start[0] + s1));
                                sb.Append('-');
                                sb.Append((char) (stop[0] - s2));
                                sb.Append(']');
                                sb.Append("[\uDC00-\uDFFF]|");
                            }

                            if (s2 != 0)
                            {
                                sb.Append(stop[0]).Append("[\uDC00-").Append(stop[1]).Append(']');
                            }
                        }
                    }
                }

                sb.Append(")");
            }

            if (singleCharRanges.Count > 0)
            {
                if (multiCharRanges.Count > 0)
                {
                    sb.Append('|');
                }

                sb.Append("[");

                for (var i = 0; i < singleCharRanges.Count; i++)
                {
                    var c = singleCharRanges[i];

                    if (c.Start == c.End)
                    {
                        sb.Append("\\u").Append(c.Start.ToString("X4"));
                    }
                    else
                    {
                        sb.Append("\\u").Append(c.Start.ToString("X4"));

                        if (c.End > c.Start + 1)
                        {
                            sb.Append('-');
                        }

                        sb.Append("\\u").Append(c.End.ToString("X4"));
                    }
                }

                sb.Append("]");
            }

            sb.Append(")");
        }

        internal record struct Range(int Start, int End);

        internal static Range[] CreateRanges(string range)
        {
            var r = new List<Range>();

            char c;
            int firstCodePoint;
            int secondCodePoint;

            for (var i = 0; i < range.Length; i++)
            {
                c = range[i];
                firstCodePoint = CodePointAt(range, i);

                if (char.IsHighSurrogate(c))
                {
                    i++;
                }

                // Special char range?
                // 
                if (c == '\\' && i + 1 < range.Length)
                {
                    c = range[++i];

                    switch (c)
                    {
                        case 'd':
                            r.Add(new Range(48, 57));
                            continue;

                        case 'D': // Not a digit (inverse of d)
                            r.Add(new Range(0, 47));
                            r.Add(new Range(58, 0x10FFFF));
                            continue;

                        case 's':

                            r.Add(new Range(9, 10));
                            r.Add(new Range(13, 13));
                            r.Add(new Range(32, 32));
                            continue;

                        case 'S': // Not a space (inverse of s)
                            r.Add(new Range(0, 8));
                            r.Add(new Range(11, 12));
                            r.Add(new Range(14, 31));
                            r.Add(new Range(33, 0x10FFFF));
                            continue;

                        case 'w':
                            r.Add(new Range(48, 57));
                            r.Add(new Range(65, 90));
                            r.Add(new Range(95, 95));
                            r.Add(new Range(97, 122));
                            continue;

                        case 'W': // Not a word letter (inverse of w)
                            r.Add(new Range(0, 47));
                            r.Add(new Range(58, 64));
                            r.Add(new Range(91, 94));
                            r.Add(new Range(96, 96));
                            r.Add(new Range(123, 0x10FFFF));
                            continue;

                        default:
                            i--;
                            break;
                    }
                }
                else if (i < range.Length - 2 && range[i + 1] == '-')
                {
                    i += 2;

                    secondCodePoint = CodePointAt(range, i);

                    if (secondCodePoint >= 0x10000)
                    {
                        i++;
                    }

                    r.Add(new Range(firstCodePoint, secondCodePoint));
                    continue;
                }

                r.Add(new Range(firstCodePoint, firstCodePoint));
            }

            if (r.Count <= 1)
            {
                return r.ToArray();
            }

            r.Sort(new Comparison<Range>(new Func<Range, Range, int>((x, y) => x.Start - y.Start)));

            // optimize

            var rNew = new List<Range>();

            var cr = r[0];
            for (var i = 1; i < r.Count; i++)
            {
                if (r[i].End <= cr.End)
                {
                    continue;
                }

                if (cr.End >= r[i].Start - 1)
                {
                    cr.End = r[i].End;
                    continue;
                }

                rNew.Add(cr);
                cr = r[i];
            }
            rNew.Add(cr);

            return rNew.ToArray();
        }

        /// <summary>
        /// Negates a Range set.  b-y -> \0-a;z-\u0x10FFFF
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        private static Range[] InvertRanges(Range[] ranges)
        {
            if (ranges.Length == 0)
            {
                return new Range[] { new Range(0, 0x10FFFF) };
            }

            var inverted = new List<Range>();

            if (ranges[0].Start > 0)
            {
                inverted.Add(new Range(0, ranges[0].Start - 1));
            }

            for (var i = 1; i < ranges.Length; i++)
            {
                inverted.Add(new Range(ranges[i - 1].End + 1, ranges[i].Start - 1));
            }

            if (ranges[ranges.Length - 1].End < 0x10FFFF)
            {
                inverted.Add(new Range(ranges[ranges.Length - 1].End + 1, 0x10FFFF));
            }

            return inverted.ToArray();
        }

        internal string EscapeFailingRegex(string pattern)
        {
            // .NET 4.x doesn't support [^] which should match any character including newline
            // c.f. https://github.com/sebastienros/esprima-dotnet/issues/146
            if (pattern.Contains("[^]"))
            {
                pattern = pattern.Replace("[^]", @"[\s\S]");
            }


            // .NET doesn't support [] which should not match any characters (inverse of [^])
            if (pattern.Contains("[]"))
            {
                // This is a temporary solution to make the parser pass. It is not a correct replacement as it will match the \0 char.
                pattern = pattern.Replace("[]", @"[\0]");
            }

            return pattern;
        }

        public Token ScanRegExpBody()
        {
            var ch = Source[Index];
            //assert(ch == '/', 'Regular expression literal must start with a slash');

            var str = GetStringBuilder();
            str.Append(Source[Index++]);
            var classMarker = false;
            var terminated = false;

            while (!Eof())
            {
                ch = Source[Index++];
                str.Append(ch);
                if (ch == '\\')
                {
                    ch = Source[Index++];
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
            var body = str.ToString();
            return new Token { Value = body.Substring(1, body.Length - 2), Literal = body };
        }

        private readonly record struct RegExpFlagsScanResult(string Flags, string Literal);
        
        private RegExpFlagsScanResult ScanRegExpFlags()
        {
            var str = "";
            var flags = "";
            while (!Eof())
            {
                var ch = Source[Index];
                if (!Character.IsIdentifierPart(ch))
                {
                    break;
                }

                ++Index;
                if (ch == '\\' && !Eof())
                {
                    ch = Source[Index];
                    if (ch == 'u')
                    {
                        ++Index;
                        var restore = Index;
                        if (ScanHexEscape('u', out ch))
                        {
                            flags += ch;
                            for (str += "\\u"; restore < Index; ++restore)
                            {
                                str += Source[restore];
                            }
                        }
                        else
                        {
                            Index = restore;
                            flags += 'u';
                            str += "\\u";
                        }

                        TolerateUnexpectedToken();
                    }
                    else
                    {
                        str += '\\';
                        TolerateUnexpectedToken();
                    }
                }
                else
                {
                    flags += ch;
                    str += ch;
                }
            }

            return new (flags, str);
        }

        public Token ScanRegExp()
        {
            var start = Index;

            var body = ScanRegExpBody();
            var (flags, literal) = ScanRegExpFlags();

            return new Token
            {
                Type = TokenType.RegularExpression,
                Value = _adaptRegexp ? ParseRegex((string) body.Value!, flags, _regexTimeout) : null,
                Literal = body.Literal + literal,
                RegexValue = new RegexValue((string) body.Value!, flags),
                LineNumber = LineNumber,
                LineStart = LineStart,
                Start = start,
                End = Index
            };
        }

        public Token Lex() => Lex(new LexOptions());

        internal Token Lex(LexOptions options)
        {
            if (Eof())
            {
                return new Token
                {
                    Type = TokenType.EOF,
                    LineNumber = LineNumber,
                    LineStart = LineStart,
                    Start = Index,
                    End = Index
                };
            }

            var cp = Source.CharCodeAt(Index);

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
                if (Character.IsDecimalDigit(Source.CharCodeAt(Index + 1)))
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

            // Possible identifier start in a surrogate pair.
            if (cp >= 0xD800 && cp < 0xDFFF)
            {
                if (char.IsLetter(Source, Index)) // Character.IsIdentifierStart(CodePointAt(Index))
                {
                    return ScanIdentifier(options.AllowIdentifierEscape);
                }
            }

            return ScanPunctuator();
        }

        public RegexOptions ParseRegexOptions(string flags)
        {
            var isGlobal = false;
            var multiline = false;
            var ignoreCase = false;
            var unicode = false;
            var sticky = false;
            var dotAll = false;

            for (var k = 0; k < flags.Length; k++)
            {
                var c = flags[k];
                if (c == 'g')
                {
                    if (isGlobal)
                    {
                        ThrowUnexpectedToken(Messages.InvalidRegExp);
                    }

                    isGlobal = true;
                }
                else if (c == 'i')
                {
                    if (ignoreCase)
                    {
                        ThrowUnexpectedToken(Messages.InvalidRegExp);
                    }

                    ignoreCase = true;
                }
                else if (c == 'm')
                {
                    if (multiline)
                    {
                        ThrowUnexpectedToken(Messages.InvalidRegExp);
                    }

                    multiline = true;
                }
                else if (c == 'u')
                {
                    if (unicode)
                    {
                        ThrowUnexpectedToken(Messages.InvalidRegExp);
                    }

                    unicode = true;
                }
                else if (c == 'y')
                {
                    if (sticky)
                    {
                        ThrowUnexpectedToken(Messages.InvalidRegExp);
                    }

                    sticky = true;
                }
                else if (c == 's')
                {
                    if (dotAll)
                    {
                        ThrowUnexpectedToken(Messages.InvalidRegExp);
                    }

                    dotAll = true;
                }
                else
                {
                    ThrowUnexpectedToken(Messages.InvalidRegExp);
                }
            }

            var options = RegexOptions.ECMAScript;

            if (multiline)
            {
                options |= RegexOptions.Multiline;
            }

            if (dotAll)
            {
                // cannot use ECMA mode with singline
                options |= RegexOptions.Singleline;
                options &= ~RegexOptions.ECMAScript;
            }

            if (ignoreCase)
            {
                options |= RegexOptions.IgnoreCase;
            }

            return options;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? FindTwoCharEqual(string input, string[] alternatives)
        {
            var c2 = input[1];
            var c1 = input[0];
            for (var i = 0; i < alternatives.Length; ++i)
            {
                var s = alternatives[i];
                if (c1 == s[0]
                    && c2 == s[1])
                {
                    return s;
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? FindThreeCharEqual(string input, string[] alternatives)
        {
            var c3 = input[2];
            var c2 = input[1];
            var c1 = input[0];
            for (var i = 0; i < alternatives.Length; ++i)
            {
                var s = alternatives[i];
                if (c1 == s[0]
                    && c2 == s[1]
                    && c3 == s[2])
                {
                    return alternatives[i];
                }
            }

            return null;
        }
    }

    public readonly struct OctalValue
    {
        public readonly int Code;
        public readonly bool Octal;

        public OctalValue(int code, bool octal)
        {
            Code = code;
            Octal = octal;
        }
    }
}
