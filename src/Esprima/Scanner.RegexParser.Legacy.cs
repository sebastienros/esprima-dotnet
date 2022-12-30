using System.Runtime.CompilerServices;
using System.Text;

namespace Esprima;

partial class Scanner
{
    partial struct RegexParser
    {
        private sealed class LegacyMode : IMode
        {
            public static readonly LegacyMode Instance = new();

            private LegacyMode() { }

            public void ProcessChar(ref AdjustPatternContext context, char ch, Action<StringBuilder, char> appender, ref RegexParser parser)
            {
                ref readonly var sb = ref context.StringBuilder;
                appender(sb, ch);
            }

            public void ProcessSetSpecialChar(ref AdjustPatternContext context, char ch)
            {
                ref readonly var sb = ref context.StringBuilder;
                sb.Append(ch);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ProcessSetChar(ref AdjustPatternContext context, char ch, Action<StringBuilder, char> appender, ref RegexParser parser, int startIndex)
            {
                ProcessSetChar(ref context, ch, ch, appender, ref parser, startIndex);
            }

            private static void ProcessSetChar(ref AdjustPatternContext context, char ch, int charCode, Action<StringBuilder, char> appender, ref RegexParser parser, int startIndex)
            {
                ref readonly var sb = ref context.StringBuilder;

                if (context.SetRangeStart >= 0)
                {
                    appender(sb, ch);
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
                            parser.MoveScannerTo(startIndex).ThrowUnexpectedToken(Messages.RegexRangeOutOfOrderInCharacterClass);
                        }
                        else
                        {
                            // Cases like /[\d-a]/ are valid but they're problematic in .NET, so they need to be escaped like @"[\d\x2Da]".
                            sb.Remove(sb.Length - 1, 1);
                            AppendCharSafe(sb, '-');

                        }
                    }

                    appender(sb, ch);
                    context.SetRangeStart = SetRangeNotStarted;
                }
            }

            public bool RewriteSet(ref AdjustPatternContext context, ref RegexParser parser)
            {
                ref readonly var sb = ref context.StringBuilder;
                ref readonly var pattern = ref parser._pattern;
                ref var i = ref context.Index;

                // [] should not match any characters.
                if (context.SetStartIndex == i - 1)
                {
                    sb.Remove(sb.Length - 1, 1).Append(MatchNothingRegex);
                    return true;
                }

                // [^] should match any character including newline.
                if (context.SetStartIndex == i - 2 && pattern[i - 1] == '^')
                {
                    sb.Remove(sb.Length - 2, 2).Append(MatchAnyCharRegex);
                    return true;
                }

                return false;
            }

            public void RewriteDot(ref AdjustPatternContext context, bool dotAll)
            {
                ref readonly var sb = ref context.StringBuilder;
                _ = dotAll ? sb.Append(MatchAnyCharRegex) : sb.Append(MatchNoNewLineRegex);
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

            public bool AllowsQuantifierAfterGroup(RegexGroupType groupType)
            {
                // Lookbehind assertion groups may not be followed by quantifiers.
                // However, lookahead assertion groups may be. RegexOptions.ECMAScript seems to handle such cases in the same way as JS.
                return groupType is not
                (
                    RegexGroupType.LookbehindAssertion or
                    RegexGroupType.NegativeLookbehindAssertion
                );
            }

            public void HandleInvalidRangeQuantifier(ref AdjustPatternContext context, ref RegexParser parser, int startIndex)
            {
                // Invalid {} quantifiers like /.{/, /.{}/, /.{-1}/, etc. are ignored. RegexOptions.ECMAScript behaves in the same way,
                // so we don't need to do anything about such cases.

                ref readonly var sb = ref context.StringBuilder;
                ref readonly var pattern = ref parser._pattern;

                sb.Append(pattern[startIndex]);

                context.FollowingQuantifierError = null;
            }

            public bool AdjustEscapeSequence(ref AdjustPatternContext context, ref RegexParser parser)
            {
                // https://262.ecma-international.org/13.0/#prod-AtomEscape

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
                                AppendCharSafe(sb, (char) charCode);
                                context.FollowingQuantifierError = null;
                            }
                            else
                            {
                                ProcessSetChar(ref context, (char) charCode, s_appendCharSafe, ref parser, startIndex);
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
                                sb.Append(ch);
                                context.FollowingQuantifierError = null;
                            }
                            else
                            {
                                ProcessSetChar(ref context, ch, s_appendChar, ref parser, startIndex);
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
                                    AppendCharSafe(sb, (char) charCode);
                                    context.FollowingQuantifierError = null;
                                }
                                else
                                {
                                    ProcessSetChar(ref context, (char) charCode, s_appendCharSafe, ref parser, startIndex);
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

                                    ProcessSetChar(ref context, (char) charCode, s_appendCharSafe, ref parser, startIndex);
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
                            sb.Append('\\').Append('\\').Append(ch);
                            context.FollowingQuantifierError = null;
                        }
                        else
                        {
                            // Unterminated/invalid cases like \c is interpreted as \\c even in character sets:
                            // /[\c]/ is equivalent to @"[\\c]" (not a typo, this does match both '\' and 'c')
                            sb.Append('\\');
                            ProcessSetChar(ref context, '\\', s_appendChar, ref parser, startIndex);
                            ProcessSetChar(ref context, ch, s_appendChar, ref parser, startIndex);
                        }
                        break;

                    // CharacterEscape (octal)
                    case '0':
                        ParseOctalEscape(pattern, ref i, out charCode);
                        if (!context.WithinSet)
                        {
                            AppendCharSafe(sb, (char) charCode);
                            context.FollowingQuantifierError = null;
                        }
                        else
                        {
                            ProcessSetChar(ref context, (char) charCode, s_appendCharSafe, ref parser, startIndex);
                        }
                        break;

                    // DecimalEscape / CharacterEscape (octal)
                    case '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                        if (!context.WithinSet)
                        {
                            // Outside character sets, numbers may be backreferences (in this case the number is interpreted as decimal).
                            if (parser.TryAdjustBackreference(ref context, startIndex))
                            {
                                if (i < 0) // conversion is impossible
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
                                AppendCharSafe(sb, ch);
                                context.FollowingQuantifierError = null;
                            }
                            else
                            {
                                ProcessSetChar(ref context, ch, s_appendCharSafe, ref parser, startIndex);
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
                            sb.Append(ch);
                            context.FollowingQuantifierError = null;
                            break;
                        }

                        if (!context.WithinSet)
                        {
                            parser.AdjustNamedBackreference(ref context, startIndex);
                            if (i < 0) // conversion is impossible
                            {
                                return false;
                            }

                            context.FollowingQuantifierError = null;
                        }
                        else
                        {
                            // \k escape sequence within character sets is not allowed
                            // (except when there are no named capturing groups; see above).
                            parser.MoveScannerTo(startIndex).ThrowUnexpectedToken(Messages.RegexInvalidEscape);
                        }
                        break;

                    // CharacterClassEscape
                    case 'd' or 'D' or 's' or 'S' or 'w' or 'W':
                        // RegexOptions.ECMAScript incorrectly interprets \s as [\f\n\r\t\v\u0020]. This doesn't align with the JS specification,
                        // which defines \s as [\f\n\r\t\v\u0020\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]. We need to adjust both \s and \S.

                        if (!context.WithinSet)
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
                            context.FollowingQuantifierError = null;
                        }
                        else
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
                            sb.Append(ch);
                            context.FollowingQuantifierError = null;
                        }
                        else
                        {
                            ProcessSetChar(ref context, ch, s_appendChar, ref parser, startIndex);
                        }
                        break;

                    default:
                        if (!context.WithinSet)
                        {
                            if (ch is '<' or 'A' or 'Z' or 'z' or 'G')
                            {
                                // These .NET-only escape sequences must be unescaped outside character sets as RegexOptions.ECMAScript doesn't handle them correctly.
                                sb.Append(ch);
                                context.FollowingQuantifierError = null;
                            }
                            else
                            {
                                sb.Append(pattern, startIndex, 2);
                                context.FollowingQuantifierError = ch is 'b' or 'B' ? Messages.RegexNothingToRepeat : null;
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

                                sb.Append('\\');
                                ProcessSetChar(ref context, ch, charCode, s_appendChar, ref parser, startIndex);
                            }
                            else
                            {
                                // Within character sets, when range starts with a \- escape sequence, RegexOptions.ECMAScript behaves weird,
                                // so we need to rewrite such cases (e.g. /[\--0]/ --> /[\x2D-0]/).
                                ProcessSetChar(ref context, ch, s_appendCharSafe, ref parser, startIndex);
                            }
                        }
                        break;
                }

                return true;
            }
        }
    }
}
