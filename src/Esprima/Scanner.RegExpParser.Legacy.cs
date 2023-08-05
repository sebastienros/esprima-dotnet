using System.Runtime.CompilerServices;
using System.Text;

namespace Esprima;

partial class Scanner
{
    partial struct RegExpParser
    {
        private sealed class LegacyMode : IMode
        {
            public static readonly LegacyMode Instance = new();

            private LegacyMode() { }

            public void ProcessChar(ref ParsePatternContext context, char ch, Action<StringBuilder, char>? appender, ref RegExpParser parser)
            {
                ref readonly var sb = ref context.StringBuilder;
                appender?.Invoke(sb!, ch);
            }

            public void ProcessSetSpecialChar(ref ParsePatternContext context, char ch)
            {
                ref readonly var sb = ref context.StringBuilder;
                sb?.Append(ch);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ProcessSetChar(ref ParsePatternContext context, char ch, Action<StringBuilder, char>? appender, ref RegExpParser parser, int startIndex)
            {
                ProcessSetChar(ref context, ch, ch, appender, ref parser, startIndex);
            }

            private static void ProcessSetChar(ref ParsePatternContext context, char ch, int charCode, Action<StringBuilder, char>? appender, ref RegExpParser parser, int startIndex)
            {
                ref readonly var sb = ref context.StringBuilder;

                if (context.SetRangeStart >= 0)
                {
                    appender?.Invoke(sb!, ch);
                    context.SetRangeStart = charCode;
                }
                else
                {
                    context.SetRangeStart = ~context.SetRangeStart;

                    if (context.SetRangeStart > charCode)
                    {
                        if (context.SetRangeStart <= Character.UnicodeLastCodePoint)
                        {
                            // Cases like /[z-a]/ are syntax error.
                            parser.ReportSyntaxError(startIndex, Messages.RegExpRangeOutOfOrderInCharacterClass);
                        }
                        else
                        {
                            // Cases like /[\d-a]/ are valid but they're problematic in .NET, so they need to be escaped like @"[\d\x2Da]".
                            if (sb is not null)
                            {
                                sb.Remove(sb.Length - 1, 1);
                                AppendCharSafe(sb, '-');
                            }
                        }
                    }

                    appender?.Invoke(sb!, ch);
                    context.SetRangeStart = SetRangeNotStarted;
                }
            }

            public bool RewriteSet(ref ParsePatternContext context, ref RegExpParser parser)
            {
                ref readonly var sb = ref context.StringBuilder;

                if (sb is not null)
                {
                    ref readonly var pattern = ref parser._pattern;
                    ref readonly var i = ref context.Index;

                    // [] should not match any characters.
                    if (context.SetStartIndex == i - 1)
                    {
                        sb.Remove(sb.Length - 1, 1).Append(MatchNoneRegex);
                        return true;
                    }

                    // [^] should match any character including newline.
                    if (context.SetStartIndex == i - 2 && pattern[i - 1] == '^')
                    {
                        sb.Remove(sb.Length - 2, 2).Append(MatchAnyRegex);
                        return true;
                    }

                    return false;
                }

                return true;
            }

            public void RewriteDot(ref ParsePatternContext context, bool dotAll)
            {
                ref readonly var sb = ref context.StringBuilder;
                if (sb is not null)
                {
                    _ = dotAll ? sb.Append(MatchAnyRegex) : sb.Append(MatchAnyButNewLineRegex);
                }
            }

            private static void ParseOctalEscape(string pattern, ref int i, out ushort charCode)
            {
                charCode = 0;
                var endIndex = Math.Min(i + 3, pattern.Length);
                do
                {
                    var newCharCode = (ushort) ((charCode << 3) + (pattern[i] - '0'));
                    if (newCharCode > 0xFF)
                    {
                        break;
                    }
                    charCode = newCharCode;
                }
                while (++i < endIndex && pattern[i] is >= '0' and <= '7');

                i--;
            }

            public bool AllowsQuantifierAfterGroup(RegExpGroupType groupType)
            {
                // Lookbehind assertion groups may not be followed by quantifiers.
                // However, lookahead assertion groups may be. RegexOptions.ECMAScript seems to handle such cases in the same way as JS.
                return groupType is not
                (
                    RegExpGroupType.LookbehindAssertion or
                    RegExpGroupType.NegativeLookbehindAssertion
                );
            }

            public void HandleInvalidRangeQuantifier(ref ParsePatternContext context, ref RegExpParser parser, int startIndex)
            {
                // Invalid {} quantifiers like /.{/, /.{}/, /.{-1}/, etc. are ignored. RegexOptions.ECMAScript behaves in the same way,
                // so we don't need to do anything about such cases.

                ref readonly var sb = ref context.StringBuilder;
                ref readonly var pattern = ref parser._pattern;

                sb?.Append(pattern[startIndex]);

                context.FollowingQuantifierError = null;
            }

            public bool AdjustEscapeSequence(ref ParsePatternContext context, ref RegExpParser parser, out ParseError? conversionError)
            {
                // https://tc39.es/ecma262/#prod-AtomEscape

                ref readonly var sb = ref context.StringBuilder;
                ref readonly var pattern = ref parser._pattern;
                ref var i = ref context.Index;

                ushort charCode;
                var startIndex = i++;
                var ch = pattern[i];
                switch (ch)
                {
                    // CharacterEscape -> RegExpUnicodeEscapeSequence
                    // CharacterEscape -> HexEscapeSequence
                    case 'u':
                    case 'x':
                        if (TryParseHexEscape(pattern, ref i, out charCode))
                        {
                            if (!context.WithinSet)
                            {
                                context.AppendCharSafe?.Invoke(sb!, (char) charCode);
                                context.FollowingQuantifierError = null;
                            }
                            else
                            {
                                ProcessSetChar(ref context, (char) charCode, context.AppendCharSafe, ref parser, startIndex);
                            }
                        }
                        else
                        {
                            // Rewrite
                            // * unterminated \x escape sequences (e.g. /\x0/ --> @"x0"),
                            // * unterminated \u escape sequences (e.g. /\u012/ --> @"u012"),
                            // * invalid \x escape sequences (e.g. /\x0y/ --> @"x0y"),
                            // * invalid \u escape sequences (e.g. /\u012y/ --> @"u012y"), including
                            // * UTF32-like invalid escape sequences (e.g. /\u{0010FFFF}/ --> @"u{0010FFFF}").
                            if (!context.WithinSet)
                            {
                                sb?.Append(ch);
                                context.FollowingQuantifierError = null;
                            }
                            else
                            {
                                ProcessSetChar(ref context, ch, context.AppendChar, ref parser, startIndex);
                            }
                        }
                        break;

                    // CharacterEscape -> c ControlLetter
                    case 'c':
                        if (i + 1 < pattern.Length)
                        {
                            if ((charCode = pattern[i + 1]) is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
                            {
                                charCode = (ushort) (charCode & 0x1Fu); // value is equal to the character code modulo 32

                                if (!context.WithinSet)
                                {
                                    context.AppendCharSafe?.Invoke(sb!, (char) charCode);
                                    context.FollowingQuantifierError = null;
                                }
                                else
                                {
                                    ProcessSetChar(ref context, (char) charCode, context.AppendCharSafe, ref parser, startIndex);
                                }
                                i++;
                                break;
                            }

                            if (context.WithinSet)
                            {
                                // Within character sets, '_' and decimal digits are also allowed.
                                if ((charCode = pattern[i + 1]) is '_' or >= '0' and <= '9')
                                {
                                    charCode = (ushort) (charCode & 0x1Fu); // value is equal to the character code modulo 32

                                    ProcessSetChar(ref context, (char) charCode, context.AppendCharSafe, ref parser, startIndex);
                                    i++;
                                    break;
                                }
                            }
                        }

                        // Rewrite
                        // * unterminated caret notation escapes (e.g. /\c/ --> @"\\c",
                        // * invalid caret notation escapes (e.g. /\ch/ --> @"\\ch").
                        // (See also https://stackoverflow.com/a/48718489/8656352)
                        if (!context.WithinSet)
                        {
                            sb?.Append('\\').Append('\\').Append(ch);
                            context.FollowingQuantifierError = null;
                        }
                        else
                        {
                            // Unterminated/invalid cases like \c is interpreted as \\c even in character sets:
                            // /[\c]/ is equivalent to @"[\\c]" (not a typo, this does match both '\' and 'c')
                            sb?.Append('\\');
                            ProcessSetChar(ref context, '\\', context.AppendChar, ref parser, startIndex);
                            ProcessSetChar(ref context, ch, context.AppendChar, ref parser, startIndex);
                        }
                        break;

                    // CharacterEscape (octal)
                    case '0':
                        ParseOctalEscape(pattern, ref i, out charCode);
                        if (!context.WithinSet)
                        {
                            context.AppendCharSafe?.Invoke(sb!, (char) charCode);
                            context.FollowingQuantifierError = null;
                        }
                        else
                        {
                            ProcessSetChar(ref context, (char) charCode, context.AppendCharSafe, ref parser, startIndex);
                        }
                        break;

                    // DecimalEscape / CharacterEscape (octal)
                    case >= '1' and <= '9':
                        if (!context.WithinSet)
                        {
                            // Outside character sets, numbers may be backreferences (in this case the number is interpreted as decimal).
                            if (parser.TryAdjustBackreference(ref context, startIndex, out conversionError))
                            {
                                if (conversionError is not null)
                                {
                                    return false;
                                }

                                context.FollowingQuantifierError = null;
                                break;
                            }
                        }

                        // When the number is not a backreference, it's an octal character code.
                        if (Character.IsOctalDigit(ch))
                        {
                            goto case '0';
                        }
                        else
                        {
                            // \8 and \9 are interpreted as plain digit characters. However, we can't simply unescape them because
                            // that might cause problems in patterns like /()\1\8/
                            if (!context.WithinSet)
                            {
                                context.AppendCharSafe?.Invoke(sb!, ch);
                                context.FollowingQuantifierError = null;
                            }
                            else
                            {
                                ProcessSetChar(ref context, ch, context.AppendCharSafe, ref parser, startIndex);
                            }
                        }
                        break;

                    // 'k' GroupName
                    case 'k':
                        if (context.CapturingGroupNames is null)
                        {
                            // When the pattern contains no named capturing group,
                            // \k escapes are ignored - but not by the .NET regex engine,
                            // so they need to be rewritten (e.g. /\k<a>/ --> @"k<a>", /[\k<a>]/ --> @"[k<a>]").
                            sb?.Append(ch);
                            context.FollowingQuantifierError = null;
                            break;
                        }

                        if (!context.WithinSet)
                        {
                            parser.AdjustNamedBackreference(ref context, startIndex, out conversionError);
                            if (conversionError is not null)
                            {
                                return false;
                            }

                            context.FollowingQuantifierError = null;
                        }
                        else
                        {
                            // \k escape sequence within character sets is not allowed
                            // (except when there are no named capturing groups; see above).
                            parser.ReportSyntaxError(startIndex, Messages.RegExpInvalidEscape);
                        }
                        break;

                    // CharacterClassEscape
                    case 'd' or 'D' or 's' or 'S' or 'w' or 'W':
                        // RegexOptions.ECMAScript incorrectly interprets \s as [\f\n\r\t\v\u0020]. This doesn't align with the JS specification,
                        // which defines \s as [\f\n\r\t\v\u0020\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]. We need to adjust both \s and \S.

                        const string InvertedWhiteSpacePattern = "\0-\u0008\u000E-\u001F\\x21-\u009F\u00A1-\u167F\u1681-\u1FFF\u200B-\u2027\u202A-\u202E\u2030-\u205E\u2060-\u2FFF\u3001-\uFEFE\uFF00-\uFFFF";

                        if (!context.WithinSet)
                        {
                            if (sb is not null)
                            {
                                if (ch == 's')
                                {
                                    sb.Append('[').Append('\\').Append(ch).Append(AdditionalWhiteSpacePattern).Append(']');
                                }
                                else if (ch == 'S')
                                {
                                    sb.Append('[').Append(InvertedWhiteSpacePattern).Append(']');
                                }
                                else
                                {
                                    sb.Append(pattern, startIndex, 2);
                                }
                            }

                            context.FollowingQuantifierError = null;
                        }
                        else
                        {
                            if (sb is not null)
                            {
                                if (context.SetRangeStart < 0)
                                {
                                    // Cases like /[a-\d]/ are valid in JS but they're problematic in .NET, so they need to be escaped like @"[a\x2D\d]".
                                    sb.Remove(sb.Length - 1, 1);
                                    AppendCharSafe(sb, '-');
                                }

                                if (ch == 's')
                                {
                                    sb.Append('\\').Append(ch).Append(AdditionalWhiteSpacePattern);
                                }
                                else if (ch == 'S')
                                {
                                    sb.Append(InvertedWhiteSpacePattern);
                                }
                                else
                                {
                                    sb.Append(pattern, startIndex, 2);
                                }
                            }

                            context.SetRangeStart = context.SetRangeStart >= 0 ? SetRangeStartedWithCharClass : SetRangeNotStarted;
                        }
                        break;

                    // \p and \P escapes are ignored - but not by the .NET regex engine,
                    // so they need to be rewritten (e.g. /\p{Sc}/ --> @"p{Sc}").
                    case 'p' or 'P':
                    // Several .NET-only escape sequences must be unescaped as RegexOptions.ECMAScript doesn't handle them correctly.
                    case 'a' or 'e':
                        if (!context.WithinSet)
                        {
                            sb?.Append(ch);
                            context.FollowingQuantifierError = null;
                        }
                        else
                        {
                            ProcessSetChar(ref context, ch, context.AppendChar, ref parser, startIndex);
                        }
                        break;

                    default:
                        if (!context.WithinSet)
                        {
                            if (ch is '<' or 'A' or 'Z' or 'z' or 'G')
                            {
                                // These .NET-only escape sequences must be unescaped outside character sets as RegexOptions.ECMAScript doesn't handle them correctly.
                                sb?.Append(ch);
                                context.FollowingQuantifierError = null;
                            }
                            else
                            {
                                sb?.Append(pattern, startIndex, 2);
                                context.FollowingQuantifierError = ch is 'b' or 'B' ? Messages.RegExpNothingToRepeat : null;
                            }
                        }
                        else
                        {
                            if (ch != '-')
                            {
                                if (!TryGetSimpleEscapeCharCode(ch, context.WithinSet, out charCode))
                                {
                                    charCode = ch;
                                }

                                sb?.Append('\\');
                                ProcessSetChar(ref context, ch, charCode, context.AppendChar, ref parser, startIndex);
                            }
                            else
                            {
                                // Within character sets, when range starts with a \- escape sequence, RegexOptions.ECMAScript behaves weird,
                                // so we need to rewrite such cases (e.g. /[\--0]/ --> /[\x2D-0]/).
                                ProcessSetChar(ref context, ch, context.AppendCharSafe, ref parser, startIndex);
                            }
                        }
                        break;
                }

                conversionError = null;
                return true;
            }
        }
    }
}
