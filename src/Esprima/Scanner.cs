﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Esprima.Ast;

namespace Esprima
{
    public class SourceLocation
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

    public class Scanner
    {
        private readonly IErrorHandler _errorHandler;
        private readonly bool _trackComment;
        private readonly bool _adaptRegexp;
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
        private bool Eof()
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
                else if (ch == 0x3C && !IsModule)
                {
                    // U+003C is '<'
                    if (Source[Index + 1] == '!'
                        && Source[Index + 2] == '-'
                        && Source[Index + 3] == '-')
                    {
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

        public int CodePointAt(int i)
        {
            //int cp = Source.CharCodeAt(i);

            //if (cp >= 0xD800 && cp <= 0xDBFF) {
            //    var second = Source.CharCodeAt(i + 1);
            //    if (second >= 0xDC00 && second <= 0xDFFF)
            //    {
            //        var first = cp;
            //        cp = (first - 0xD800) * 0x400 + second - 0xDC00 + 0x10000;
            //    }
            //}

            return char.ConvertToUtf32(Source, i);
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
            var cp = CodePointAt(Index);
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
                cp = CodePointAt(Index);
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

        public Token ScanIdentifier()
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
                TolerateUnexpectedToken(Messages.InvalidEscapedReservedWord);
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
            var number = this.ScanLiteralPart(Character.IsHexDigit, out var hasUpperCase);

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
                    return ScanBigIntLiteral(start, number, NumberStyles.HexNumber);
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

        private Token ScanBigIntLiteral(int start, ReadOnlySpan<char> number, NumberStyles style)
        {
            var c = number[0];
            if (c > 7 && Character.IsHexDigit(c))
            {
                // ensure we get positive number
                number = ("0" + number.ToString()).AsSpan();
            }
#if HAS_SPAN_PARSE
            var bigInt = BigInteger.Parse(number, style);
#else
            var bigInt = BigInteger.Parse(number.ToString(), style);
#endif
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
            var number = this.ScanLiteralPart(static c => c is '0' or '1', out _).ToString();

            if (number.Length == 0)
            {
                // only 0b or 0B
                ThrowUnexpectedToken();
            }

            if (!Eof())
            {
                var ch = Source.CharCodeAt(Index);
                /* istanbul ignore else */
                if (Character.IsIdentifierStart(ch) || Character.IsDecimalDigit(ch))
                {
                    ThrowUnexpectedToken();
                }
            }

            var token = new Token
            {
                Value = number,
                LineNumber = LineNumber,
                LineStart = LineStart,
                Start = start,
                End = Index
            };

            if (number.Length > 4)
            {
                token.Type = TokenType.BigIntLiteral;
                token.BigIntValue = BinaryToBigInteger(number);
            }
            else
            {
                token.Type = TokenType.NumericLiteral;
                token.NumericValue = Convert.ToUInt32(number, 2);
            }

            return token;
        }

        private BigInteger BinaryToBigInteger(string value)
        {
            BigInteger res = 0;
            foreach(var c in value)
            {
                res <<= 1;
                res += c == '1' ? 1 : 0;
            }

            return res;
        }

        public Token ScanOctalLiteral(char prefix, int start)
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

            sb.Append(this.ScanLiteralPart(Character.IsOctalDigit, out _));
            var number = sb.ToString();

            if (!octal && number.Length == 0)
            {
                // only 0o or 0O
                ThrowUnexpectedToken();
            }

            if (Character.IsIdentifierStart(Source.CharCodeAt(Index)) || Character.IsDecimalDigit(Source.CharCodeAt(Index)))
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

        private ReadOnlySpan<char> ScanLiteralPart(Func<char, bool> check, out bool hasUpperCase)
        {
            hasUpperCase = false;
            var start = Index;

            var charCode = Source.CharCodeAt(Index);
            if (charCode == '_')
            {
                ThrowUnexpectedToken(Messages.NumericSeperatorNotAllowedHere);
            }

            var needsCleanup = false;
            while (check(charCode) || charCode == '_')
            {
                if (charCode == '_')
                {
                    needsCleanup = true;
                }

                hasUpperCase |= char.IsUpper(charCode);

                Index++;
                var newCharCode = Source.CharCodeAt(Index);
                if (charCode == '_' && newCharCode == '_')
                {
                    ThrowUnexpectedToken(Messages.NumericSeperatorOneUnderscore);
                }

                if (Eof())
                {
                    break;
                }
                charCode = newCharCode;
            }

            if (charCode == '_')
            {
                ThrowUnexpectedToken(Messages.NumericSeperatorNotAllowedHere);
            }

            var span = Source.AsSpan(start, Index - start);
            return needsCleanup
                ? span.ToString().Replace("_", "").AsSpan()
                : span;
        }

        public Token ScanNumericLiteral()
        {
            var sb = GetStringBuilder();
            var start = Index;
            var ch = Source[start];
            //assert(Character.IsDecimalDigit(ch) || (ch == '.'),
            //    'Numeric literal must start with a decimal digit or a decimal point');

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
                    if (ch == 'x' || ch == 'X')
                    {
                        ++Index;
                        return ScanHexLiteral(start);
                    }

                    if (ch == 'b' || ch == 'B')
                    {
                        ++Index;
                        return ScanBinaryLiteral(start);
                    }

                    if (ch == 'o' || ch == 'O')
                    {
                        return ScanOctalLiteral(ch, start);
                    }

                    if (ch > 0 && Character.IsOctalDigit(ch))
                    {
                        if (IsImplicitOctalLiteral())
                        {
                            return ScanOctalLiteral(ch, start);
                        }
                    }
                }

                --Index;
                sb.Append(this.ScanLiteralPart(Character.IsDecimalDigit, out _));
                ch = Source.CharCodeAt(Index);
            }

            if (ch == '.')
            {
                sb.Append(Source[Index++]);
                sb.Append(this.ScanLiteralPart(Character.IsDecimalDigit, out _));

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
                    sb.Append(this.ScanLiteralPart(Character.IsDecimalDigit, out _));
                }
                else
                {
                    ThrowUnexpectedToken();
                }
            }
            else if (ch == 'n')
            {
                Index++;
                return ScanBigIntLiteral(start, sb.ToString().AsSpan(), NumberStyles.Integer);
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

        public Token ScanStringLiteral()
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
                                TolerateUnexpectedToken();
                                break;

                            default:
                                if (ch != char.MinValue && Character.IsOctalDigit(ch))
                                {
                                    var octToDec = OctalToDecimal(ch);

                                    octal = octToDec.Octal || octal;
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

        // https://tc39.github.io/ecma262/#sec-literals-regular-expression-literals

        public Regex? TestRegExp(string pattern, string flags)
        {
            // The BMP character to use as a replacement for astral symbols when
            // translating an ES6 "u"-flagged pattern to an ES5-compatible
            // approximation.
            // Note: replacing with '\uFFFF' enables false positives in unlikely
            // scenarios. For example, `[\u{1044f}-\u{10440}]` is an invalid
            // pattern that would not be detected by this substitution.
            var astralSubstitute = "\uFFFF";
            var tmp = pattern;
            var self = this;

            if (flags.IndexOf('u') >= 0)
            {
                tmp = Regex
                    // Replace every Unicode escape sequence with the equivalent
                    // BMP character or a constant ASCII code point in the case of
                    // astral symbols. (See the above note on `astralSubstitute`
                    // for more information.)
                    .Replace(tmp, @"\\u\{([0-9a-fA-F]+)\}|\\u([a-fA-F0-9]{4})", (match) =>
                    {
                        int codePoint;
                        if (!string.IsNullOrEmpty(match.Groups[1].Value))
                        {
                            codePoint = Convert.ToInt32(match.Groups[1].Value, 16);
                        }
                        else
                        {
                            codePoint = Convert.ToInt32(match.Groups[2].Value, 16);
                        }

                        if (codePoint > 0x10FFFF)
                        {
                            ThrowUnexpectedToken(Messages.InvalidRegExp);
                        }

                        if (codePoint <= 0xFFFF)
                        {
                            return ParserExtensions.CharToString((char) codePoint);
                        }

                        return astralSubstitute;
                    });

                // Replace each paired surrogate with a single ASCII symbol to
                // avoid throwing on regular expressions that are only valid in
                // combination with the "u" flag.
                tmp = Regex.Replace(tmp, "[\uD800-\uDBFF][\uDC00-\uDFFF]", astralSubstitute);
            }

            // First, detect invalid regular expressions.
            var options = ParseRegexOptions(flags);

            try
            {
                new Regex(tmp, options);
            }
            catch
            {
                tmp = EscapeFailingRegex(tmp);

                try
                {
                    new Regex(tmp, options);
                }
                catch
                {
                    ThrowUnexpectedToken(Messages.InvalidRegExp);
                }
            }

            // Return a regular expression object for this pattern-flag pair, or
            // `null` in case the current environment doesn't support the flags it
            // uses.
            try
            {
                // Do we need to convert the expression to its .NET equivalent?
                if (_adaptRegexp && options.HasFlag(RegexOptions.Multiline))
                {
                    // Replace all non-escaped $ occurences by \r?$
                    // c.f. http://programmaticallyspeaking.com/regular-expression-multiline-mode-whats-a-newline.html

                    var index = 0;
                    var newPattern = pattern;
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

                    pattern = newPattern;
                }

                return new Regex(pattern, options);
            }
            catch
            {
                return null;
            }
        }

        public string EscapeFailingRegex(string pattern)
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

        public Token ScanRegExpFlags()
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

            return new Token { Value = flags, Literal = str };
        }

        public Token ScanRegExp()
        {
            var start = Index;

            var body = ScanRegExpBody();
            var flags = ScanRegExpFlags();
            var flagsValue = (string) flags.Value!;
            var value = TestRegExp((string) body.Value!, flagsValue);

            return new Token
            {
                Type = TokenType.RegularExpression,
                Value = value,
                Literal = body.Literal + flags.Literal,
                RegexValue = new RegexValue((string) body.Value!, flagsValue),
                LineNumber = LineNumber,
                LineStart = LineStart,
                Start = start,
                End = Index
            };
        }

        public Token Lex()
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
                return ScanIdentifier();
            }

            // Very common: ( and ) and ;
            if (cp == 0x28 || cp == 0x29 || cp == 0x3B)
            {
                return ScanPunctuator();
            }

            // String literal starts with single quote (U+0027) or double quote (U+0022).
            if (cp == 0x27 || cp == 0x22)
            {
                return ScanStringLiteral();
            }

            // Dot (.) U+002E can also start a floating-point number, hence the need
            // to check the next character.
            if (cp == 0x2E)
            {
                if (Character.IsDecimalDigit(Source.CharCodeAt(Index + 1)))
                {
                    return ScanNumericLiteral();
                }

                return ScanPunctuator();
            }

            if (Character.IsDecimalDigit(cp))
            {
                return ScanNumericLiteral();
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
                    return ScanIdentifier();
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
