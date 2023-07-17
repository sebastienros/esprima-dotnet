using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Esprima;

partial class Scanner
{
    partial struct RegexParser
    {
        private const string MatchSurrogatePairRegex = "[\uD800-\uDBFF][\uDC00-\uDFFF]";

        private sealed class UnicodeMode : IMode
        {
            public static readonly UnicodeMode Instance = new();

            private UnicodeMode() { }

            public void ProcessChar(ref ParsePatternContext context, char ch, Action<StringBuilder, char>? appender, ref RegexParser parser)
            {
                ref readonly var sb = ref context.StringBuilder;
                ref readonly var pattern = ref parser._pattern;
                ref var i = ref context.Index;

                if (!char.IsSurrogate(ch))
                {
                    appender?.Invoke(sb!, ch);
                }
                else if (char.IsHighSurrogate(ch) && char.IsLowSurrogate(pattern.CharCodeAt(i + 1)))
                {
                    // Surrogate pairs should be surrounded by a non-capturing group to act as one character (because of cases like /💩+/u).
                    sb?.Append("(?:").Append(ch).Append(pattern[i + 1]).Append(')');
                    i++;
                }
                else
                {
                    if (sb is not null)
                    {
                        AppendLoneSurrogate(sb, ch);
                    }
                }
            }

            private static void AppendCodePointSafe(StringBuilder sb, int cp)
            {
                if (cp > char.MaxValue)
                {
                    var s = char.ConvertFromUtf32(cp);
                    // Surrogate pairs should be surrounded by a non-capturing group to act as one character (because of cases like /\ud83d\udca9+/u).
                    sb.Append("(?:").Append(s[0]).Append(s[1]).Append(')');
                }
                else
                {
                    Debug.Assert(cp >= 0, "Invalid code point.");
                    AppendUnicodeCharSafe(sb, (char) cp);
                }
            }

            private static void AppendUnicodeCharSafe(StringBuilder sb, char ch)
            {
                if (!char.IsSurrogate(ch))
                {
                    AppendCharSafe(sb, ch);
                }
                else
                {
                    AppendLoneSurrogate(sb, ch);
                }
            }

            private static void AppendLoneSurrogate(StringBuilder sb, char ch)
            {
                if (sb is not null)
                {
                    // Lone surrogates must not match parts of surrogate pairs
                    // (see https://exploringjs.com/es6/ch_regexp.html#_consequence-lone-surrogates-in-the-regular-expression-only-match-lone-surrogates).
                    // We can simulate this using negative lookbehind/lookahead.

                    sb.Append("(?:");
                    _ = char.IsHighSurrogate(ch)
                        ? sb.Append(ch).Append("(?![\uDC00-\uDFFF])")
                        : sb.Append("(?<![\uD800-\uDBFF])").Append(ch);
                    sb.Append(')');
                }
            }

            public void ProcessSetSpecialChar(ref ParsePatternContext context, char ch) { }

            public void ProcessSetChar(ref ParsePatternContext context, char ch, Action<StringBuilder, char>? appender, ref RegexParser parser, int startIndex)
            {
                ref readonly var pattern = ref parser._pattern;
                ref var i = ref context.Index;

                if (char.IsHighSurrogate(ch) && char.IsLowSurrogate(pattern.CharCodeAt(i + 1)))
                {
                    // Surrogate pairs should be surrounded by a non-capturing group to act as one character.
                    AddSetCodePoint(ref context, char.ConvertToUtf32(ch, pattern[i + 1]), ref parser, startIndex);
                    i++;
                }
                else
                {
                    AddSetCodePoint(ref context, ch, ref parser, startIndex);
                }
            }

            private static void AddSetCodePoint(ref ParsePatternContext context, int cp, ref RegexParser parser, int startIndex)
            {
                Debug.Assert(cp is >= 0 and <= Character.UnicodeLastCodePoint, "Invalid end code point.");

                var sb = context.StringBuilder;

                if (context.SetRangeStart >= 0)
                {
                    if (sb is not null)
                    {
                        context.UnicodeSet.Add(new CodePointRange(cp));
                    }

                    context.SetRangeStart = cp;
                }
                else
                {
                    context.SetRangeStart = ~context.SetRangeStart;

                    // Cases like /[z-a]/u or /[\d-a]/u are syntax error.
                    if (context.SetRangeStart > cp)
                    {
                        parser.ReportSyntaxError(startIndex, context.SetRangeStart <= Character.UnicodeLastCodePoint
                            ? Messages.RegexRangeOutOfOrderInCharacterClass
                            : Messages.RegexInvalidCharacterClass);
                    }

                    if (sb is not null)
                    {
                        context.UnicodeSet.AsSpan().Last() = new CodePointRange(context.SetRangeStart, cp);
                    }

                    context.SetRangeStart = SetRangeNotStarted;
                }
            }

            public bool RewriteSet(ref ParsePatternContext context, ref RegexParser parser)
            {
                ref readonly var sb = ref context.StringBuilder;
                ref readonly var pattern = ref parser._pattern;

                if (sb is not null)
                {
                    if (context.SetRangeStart < 0)
                    {
                        context.UnicodeSet.Add(new CodePointRange('-'));
                    }

                    CodePointRange.NormalizeRanges(ref context.UnicodeSet);

                    AppendSet(sb, context.UnicodeSet.AsSpan(), isInverted: pattern.CharCodeAt(context.SetStartIndex + 1) == '^');

                    context.UnicodeSet = default;
                }

                return true;
            }

            private static void AppendSet(StringBuilder sb, ReadOnlySpan<CodePointRange> normalizedSet, bool isInverted)
            {
                // 0. Handle edge cases

                switch (
                    normalizedSet.Length == 0 ? isInverted :
                    normalizedSet.Length == 1 && normalizedSet[0].Start == 0 && normalizedSet[0].End == Character.UnicodeLastCodePoint ? !isInverted :
                    (bool?) null)
                {
                    case false:
                        sb.Append(MatchNothingRegex);
                        return;
                    case true:
                        sb.Append("(?:").Append(MatchSurrogatePairRegex).Append('|').Append(MatchAnyCharRegex).Append(')');
                        return;
                }

                // 1. Split set into BMP and astral parts

                int i;
                for (i = 0; i < normalizedSet.Length; i++)
                {
                    ref readonly var range = ref normalizedSet[i];
                    if (range.Start > char.MaxValue)
                    {
                        break;
                    }
                    else if (range.End > char.MaxValue)
                    {
                        i++;
                        break;
                    }
                }

                Span<CodePointRange> rangeSpan;
                ArrayList<CodePointRange> bmpRanges;
                if (i != 0)
                {
                    bmpRanges = new ArrayList<CodePointRange>(new CodePointRange[i]);
                    rangeSpan = bmpRanges.AsSpan();
                    normalizedSet.Slice(0, rangeSpan.Length).CopyTo(rangeSpan);

                    ref var range = ref rangeSpan.Last();
                    if (range.End > char.MaxValue)
                    {
                        range = new CodePointRange(range.Start, char.MaxValue);
                        i--;
                    }
                }
                else
                {
                    bmpRanges = default;
                }

                ArrayList<CodePointRange> astralRanges;
                if (i != normalizedSet.Length)
                {
                    astralRanges = new ArrayList<CodePointRange>(new CodePointRange[normalizedSet.Length - i]);
                    rangeSpan = astralRanges.AsSpan();
                    normalizedSet.Slice(i, rangeSpan.Length).CopyTo(rangeSpan);

                    ref var range = ref rangeSpan[0];
                    if (range.Start <= char.MaxValue)
                    {
                        range = new CodePointRange(char.MaxValue + 1, range.End);
                    }
                }
                else
                {
                    astralRanges = default;
                }

                if (isInverted)
                {
                    astralRanges = CodePointRange.InvertRanges(astralRanges.AsSpan(), start: char.MaxValue + 1);
                }

                // 3. Lone surrogates need special care: we need to handle ranges which contains surrogates separately

                ArrayList<CodePointRange> loneHighSurrogateRanges = default;
                ArrayList<CodePointRange> loneLowSurrogateRanges = default;
                for (i = 0; i < bmpRanges.Count; i++)
                {
                    var range = bmpRanges[i];

                    if (range.End < 0xD800)
                    {
                        continue;
                    }

                    if (range.Start > 0xDFFF)
                    {
                        break;
                    }

                    if (range.End > 0xDFFF)
                    {
                        if (range.Start < 0xD800)
                        {
                            bmpRanges[i] = new CodePointRange(range.Start, 0xD800 - 1);
                            bmpRanges.Insert(++i, new CodePointRange(0xDFFF + 1, range.End));
                            range = new CodePointRange(0xD800, 0xDFFF);
                        }
                        else
                        {
                            bmpRanges[i] = new CodePointRange(0xDFFF + 1, range.End);
                            range = new CodePointRange(range.Start, 0xDFFF);
                        }
                    }
                    else
                    {
                        if (range.Start < 0xD800)
                        {
                            bmpRanges[i] = new CodePointRange(range.Start, 0xD800 - 1);
                            range = new CodePointRange(0xD800, range.End);
                        }
                        else
                        {
                            bmpRanges.RemoveAt(i--);
                        }
                    }

                    if (range.End >= 0xDC00 && range.Start < 0xDC00)
                    {
                        loneHighSurrogateRanges.Add(new CodePointRange(range.Start, 0xDC00 - 1));
                        loneLowSurrogateRanges.Add(new CodePointRange(0xDC00, range.End));
                    }
                    else if (range.Start < 0xDC00)
                    {
                        loneHighSurrogateRanges.Add(range);
                    }
                    else
                    {
                        loneLowSurrogateRanges.Add(range);
                    }
                }

                if (isInverted)
                {
                    bmpRanges.Add(new CodePointRange(0xD800, 0xDFFF));
                    loneHighSurrogateRanges = CodePointRange.InvertRanges(loneHighSurrogateRanges.AsSpan(), start: 0xD800, end: 0xDBFF);
                    loneLowSurrogateRanges = CodePointRange.InvertRanges(loneLowSurrogateRanges.AsSpan(), start: 0xDC00, end: 0xDFFF);
                }

                // 4. Append ranges

                sb.Append("(?:");

                string? separator = null;

                if (astralRanges.Count > 0)
                {
                    rangeSpan = astralRanges.AsSpan();
                    for (i = 0; i < rangeSpan.Length; i++)
                    {
                        sb.Append(separator);
                        separator = "|";

                        ref readonly var range = ref rangeSpan[i];

                        if (range.Start == range.End)
                        {
                            sb.Append(char.ConvertFromUtf32(range.Start));
                        }
                        else
                        {
                            Debug.Assert(range.Start <= range.End);

                            var start = char.ConvertFromUtf32(range.Start);
                            var end = char.ConvertFromUtf32(range.End);

                            if (start[0] == end[0])
                            {
                                sb.Append(start[0]);
                                AppendAstralRange(sb, new CodePointRange(start[1], end[1]));
                            }
                            else
                            {
                                var s1 = start[1] > 0xDC00 ? 1 : 0;
                                var s2 = end[1] < 0xDFFF ? 1 : 0;
                                string? innerSeparator = null;

                                if (s1 != 0)
                                {
                                    sb.Append(start[0]);
                                    AppendAstralRange(sb, new CodePointRange(start[1], 0xDFFF));
                                    innerSeparator = "|";
                                }
                                if (end[0] - start[0] >= s1 + s2)
                                {
                                    sb.Append(innerSeparator);
                                    AppendAstralRange(sb, new CodePointRange(start[0] + s1, end[0] - s2));
                                    AppendAstralRange(sb, new CodePointRange(0xDC00, 0xDFFF));
                                    innerSeparator = "|";
                                }
                                if (s2 != 0)
                                {
                                    sb.Append(innerSeparator);
                                    sb.Append(end[0]);
                                    AppendAstralRange(sb, new CodePointRange(0xDC00, end[1]));
                                }
                            }
                        }
                    }
                }

                if (loneHighSurrogateRanges.Count > 0)
                {
                    sb.Append(separator);
                    separator = "|";

                    sb.Append('[');

                    rangeSpan = loneHighSurrogateRanges.AsSpan();
                    for (i = 0; i < rangeSpan.Length; i++)
                    {
                        AppendBmpRange(sb, rangeSpan[i], s_appendChar);
                    }

                    sb.Append(']').Append("(?![\uDC00-\uDFFF])");
                }

                if (loneLowSurrogateRanges.Count > 0)
                {
                    sb.Append(separator);
                    separator = "|";

                    sb.Append("(?<![\uD800-\uDBFF])").Append('[');

                    rangeSpan = loneLowSurrogateRanges.AsSpan();
                    for (i = 0; i < rangeSpan.Length; i++)
                    {
                        AppendBmpRange(sb, rangeSpan[i], s_appendChar);
                    }

                    sb.Append(']');
                }

                if (bmpRanges.Count > 0)
                {
                    sb.Append(separator);

                    sb.Append('[');

                    if (isInverted)
                    {
                        sb.Append("^");
                    }

                    rangeSpan = bmpRanges.AsSpan();
                    for (i = 0; i < rangeSpan.Length; i++)
                    {
                        AppendBmpRange(sb, rangeSpan[i], s_appendCharSafe);
                    }

                    sb.Append(']');
                }

                sb.Append(')');

                static void AppendRange(StringBuilder sb, CodePointRange range, Action<StringBuilder, char> appender, Action<StringBuilder> onRangeStart, Action<StringBuilder> onRangeEnd)
                {
                    if (range.Start == range.End)
                    {
                        appender(sb, (char) range.Start);
                    }
                    else
                    {
                        onRangeStart(sb);
                        appender(sb, (char) range.Start);
                        if (range.End > range.Start + 1)
                        {
                            sb.Append('-');
                        }
                        appender(sb, (char) range.End);
                        onRangeEnd(sb);
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static void AppendBmpRange(StringBuilder sb, CodePointRange range, Action<StringBuilder, char> appender) => AppendRange(sb, range, appender, static _ => { }, static _ => { });

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static void AppendAstralRange(StringBuilder sb, CodePointRange range) => AppendRange(sb, range, s_appendChar, static sb => sb.Append('['), static sb => sb.Append(']'));
            }

            private static void AddStandardCharClass(ref ArrayList<CodePointRange> set, char ch)
            {
                // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Regular_expressions/Character_classes#types

                switch (ch)
                {
                    case 'd': // [0-9]
                        set.Add(new CodePointRange(0x0030, 0x0039));
                        break;
                    case 'D': // [^0-9]
                        set.Add(new CodePointRange(0x0000, 0x002F));
                        set.Add(new CodePointRange(0x003A, Character.UnicodeLastCodePoint));
                        break;
                    case 's': // [\f\n\r\t\v\u0020\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]
                        set.Add(new CodePointRange(0x0009, 0x000D));
                        set.Add(new CodePointRange(0x0020));
                        set.Add(new CodePointRange(0x00A0));
                        set.Add(new CodePointRange(0x1680));
                        set.Add(new CodePointRange(0x2000, 0x200A));
                        set.Add(new CodePointRange(0x2028, 0x2029));
                        set.Add(new CodePointRange(0x202F));
                        set.Add(new CodePointRange(0x205F));
                        set.Add(new CodePointRange(0x3000));
                        set.Add(new CodePointRange(0xFEFF));
                        break;
                    case 'S': // [^\f\n\r\t\v\u0020\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]
                        set.Add(new CodePointRange(0x0000, 0x0008));
                        set.Add(new CodePointRange(0x000E, 0x001F));
                        set.Add(new CodePointRange(0x0021, 0x009F));
                        set.Add(new CodePointRange(0x00A1, 0x167F));
                        set.Add(new CodePointRange(0x1681, 0x1FFF));
                        set.Add(new CodePointRange(0x200B, 0x2027));
                        set.Add(new CodePointRange(0x202A, 0x202E));
                        set.Add(new CodePointRange(0x2030, 0x205E));
                        set.Add(new CodePointRange(0x2060, 0x2FFF));
                        set.Add(new CodePointRange(0x3001, 0xFEFE));
                        set.Add(new CodePointRange(0xFF00, Character.UnicodeLastCodePoint));
                        break;
                    case 'w': // [A-Za-z0-9_]
                        set.Add(new CodePointRange(0x0030, 0x0039));
                        set.Add(new CodePointRange(0x0041, 0x005A));
                        set.Add(new CodePointRange(0x005F));
                        set.Add(new CodePointRange(0x0061, 0x007A));
                        break;
                    case 'W': // [^A-Za-z0-9_]
                        set.Add(new CodePointRange(0x0000, 0x002F));
                        set.Add(new CodePointRange(0x003A, 0x0040));
                        set.Add(new CodePointRange(0x005B, 0x005E));
                        set.Add(new CodePointRange(0x0060));
                        set.Add(new CodePointRange(0x007B, Character.UnicodeLastCodePoint));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ch), ch, null);
                }
            }

            private static bool TryTranslateUnicodePropertyToRanges(ReadOnlySpan<char> expression, CodePointRange.Cache cache, [MaybeNullWhen(false)] out CodePointRange[] ranges)
            {
                var index = expression.IndexOf('=');
                if (index >= 0)
                {
                    var propertyName = expression.Slice(0, index);

                    // https://262.ecma-international.org/13.0/#table-nonbinary-unicode-properties
                    if (!(propertyName.Equals("gc".AsSpan(), StringComparison.Ordinal)
                          || propertyName.Equals("General_Category".AsSpan(), StringComparison.Ordinal)))
                    {
                        ranges = null;
                        return false;
                    }
                    expression = expression.Slice(index + 1);
                }

                ranges = GetUnicodeCategoryRanges(expression, cache);
                return ranges is not null;
            }

            private static CodePointRange[]? GetUnicodeCategoryRanges(ReadOnlySpan<char> name, CodePointRange.Cache cache)
            {
                // https://262.ecma-international.org/13.0/#table-unicode-general-category-values
                // https://unicode.org/reports/tr18/#General_Category_Property

                return name switch
                {
                    "L" or "Letter" => cache.GetGeneralCategory(CodePointRange.Cache.LetterCategory),
                    "LC" or "Cased_Letter" => cache.GetGeneralCategory(CodePointRange.Cache.CasedLetterCategory),
                    "Lu" or "Uppercase_Letter" => cache.GetGeneralCategory(UnicodeCategory.UppercaseLetter),
                    "Ll" or "Lowercase_Letter" => cache.GetGeneralCategory(UnicodeCategory.LowercaseLetter),
                    "Lt" or "Titlecase_Letter" => cache.GetGeneralCategory(UnicodeCategory.TitlecaseLetter),
                    "Lm" or "Modifier_Letter" => cache.GetGeneralCategory(UnicodeCategory.ModifierLetter),
                    "Lo" or "Other_Letter" => cache.GetGeneralCategory(UnicodeCategory.OtherLetter),

                    "M" or "Mark" or "Combining_Mark" => cache.GetGeneralCategory(CodePointRange.Cache.MarkCategory),
                    "Mn" or "Nonspacing_Mark" => cache.GetGeneralCategory(UnicodeCategory.NonSpacingMark),
                    "Mc" or "Spacing_Mark" => cache.GetGeneralCategory(UnicodeCategory.SpacingCombiningMark),
                    "Me" or "Enclosing_Mark" => cache.GetGeneralCategory(UnicodeCategory.EnclosingMark),

                    "N" or "Number" => cache.GetGeneralCategory(CodePointRange.Cache.NumberCategory),
                    "Nd" or "Decimal_Number" or "digit" => cache.GetGeneralCategory(UnicodeCategory.DecimalDigitNumber),
                    "Nl" or "Letter_Number" => cache.GetGeneralCategory(UnicodeCategory.LetterNumber),
                    "No" or "Other_Number" => cache.GetGeneralCategory(UnicodeCategory.OtherNumber),

                    "P" or "Punctuation" or "punct" => cache.GetGeneralCategory(CodePointRange.Cache.PunctuationCategory),
                    "Pc" or "Connector_Punctuation" => cache.GetGeneralCategory(UnicodeCategory.ConnectorPunctuation),
                    "Pd" or "Dash_Punctuation" => cache.GetGeneralCategory(UnicodeCategory.DashPunctuation),
                    "Ps" or "Open_Punctuation" => cache.GetGeneralCategory(UnicodeCategory.OpenPunctuation),
                    "Pe" or "Close_Punctuation" => cache.GetGeneralCategory(UnicodeCategory.ClosePunctuation),
                    "Pi" or "Initial_Punctuation" => cache.GetGeneralCategory(UnicodeCategory.InitialQuotePunctuation),
                    "Pf" or "Final_Punctuation" => cache.GetGeneralCategory(UnicodeCategory.FinalQuotePunctuation),
                    "Po" or "Other_Punctuation" => cache.GetGeneralCategory(UnicodeCategory.OtherPunctuation),

                    "S" or "Symbol" => cache.GetGeneralCategory(CodePointRange.Cache.SymbolCategory),
                    "Sm" or "Math_Symbol" => cache.GetGeneralCategory(UnicodeCategory.MathSymbol),
                    "Sc" or "Currency_Symbol" => cache.GetGeneralCategory(UnicodeCategory.CurrencySymbol),
                    "Sk" or "Modifier_Symbol" => cache.GetGeneralCategory(UnicodeCategory.ModifierSymbol),
                    "So" or "Other_Symbol" => cache.GetGeneralCategory(UnicodeCategory.OtherSymbol),

                    "Z" or "Separator" => cache.GetGeneralCategory(CodePointRange.Cache.SeparatorCategory),
                    "Zs" or "Space_Separator" => cache.GetGeneralCategory(UnicodeCategory.SpaceSeparator),
                    "Zl" or "Line_Separator" => cache.GetGeneralCategory(UnicodeCategory.LineSeparator),
                    "Zp" or "Paragraph_Separator" => cache.GetGeneralCategory(UnicodeCategory.ParagraphSeparator),

                    "C" or "Other" => cache.GetGeneralCategory(CodePointRange.Cache.OtherCategory),
                    "Cc" or "Control" or "cntrl" => cache.GetGeneralCategory(UnicodeCategory.Control),
                    "Cf" or "Format" => cache.GetGeneralCategory(UnicodeCategory.Format),
                    "Cs" or "Surrogate" => cache.GetGeneralCategory(UnicodeCategory.Surrogate),
                    "Co" or "Private_Use" => cache.GetGeneralCategory(UnicodeCategory.PrivateUse),
                    "Cn" or "Unassigned" => cache.GetGeneralCategory(UnicodeCategory.OtherNotAssigned),

                    _ => null
                };
            }

            public void RewriteDot(ref ParsePatternContext context, bool dotAll)
            {
                ref readonly var sb = ref context.StringBuilder;
                if (sb is not null)
                {
                    // '.' has to be adjusted to also match all surrogate pairs.

                    // NOTE: It's important to match surrogate pairs with the first alternate to prevent matching the high and low surrogates separately
                    // (see also https://stackoverflow.com/a/18017758).

                    sb.Append("(?:").Append(MatchSurrogatePairRegex).Append('|');
                    _ = dotAll ? sb.Append(MatchAnyCharRegex) : sb.Append(MatchNoNewLineRegex);
                    sb.Append(')');
                }
            }

            public bool AllowsQuantifierAfterGroup(RegexGroupType groupType)
            {
                // Assertion groups may not be followed by quantifiers.
                return groupType is not
                (
                    RegexGroupType.LookaheadAssertion or
                    RegexGroupType.NegativeLookaheadAssertion or
                    RegexGroupType.LookbehindAssertion or
                    RegexGroupType.NegativeLookbehindAssertion
                );
            }

            public void HandleInvalidRangeQuantifier(ref ParsePatternContext context, ref RegexParser parser, int startIndex)
            {
                parser.ReportSyntaxError(startIndex, Messages.RegexIncompleteQuantifier);
            }

            public bool AdjustEscapeSequence(ref ParsePatternContext context, ref RegexParser parser)
            {
                // https://262.ecma-international.org/13.0/#prod-AtomEscape

                ref readonly var sb = ref context.StringBuilder;
                ref readonly var pattern = ref parser._pattern;
                ref var i = ref context.Index;

                ushort charCode, charCode2;
                int codePoint;
                var startIndex = i++;
                int endIndex;
                var ch = pattern[i];
                switch (ch)
                {
                    // CharacterEscape -> RegExpUnicodeEscapeSequence -> u{ CodePoint }
                    case 'u' when pattern.CharCodeAt(i + 1) == '{':
                        // Rewrite \u{...} escape sequences as follows:
                        // * /\u{7F}/u --> @"\x7F"
                        // * /\u{FFFF}/u --> "\uFFFF" (+ negative lookahead/lookbehind in the case of lone surrogates)
                        // * /\u{1F4A9}/u --> "\uD83D\uDCA9"

                        endIndex = pattern.IndexOf('}', i + 2);
                        if (endIndex < 0)
                        {
                            parser.ReportSyntaxError(startIndex, Messages.RegexInvalidUnicodeEscape);
                        }

                        var slice = pattern.AsSpan(i + 2, endIndex - (i + 2));
                        if (!int.TryParse(slice.ToParsable(), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out codePoint)
                            // NOTE: int.TryParse with NumberStyles.AllowHexSpecifier may return a negative number (e.g. '8000000' -> -2147483648)!
                            || codePoint is < 0 or > Character.UnicodeLastCodePoint)
                        {
                            parser.ReportSyntaxError(startIndex, Messages.RegexInvalidUnicodeEscape);
                        }

                        if (!context.WithinSet)
                        {
                            if (sb is not null)
                            {
                                AppendCodePointSafe(sb, codePoint);
                            }

                            context.FollowingQuantifierError = null;
                        }
                        else
                        {
                            AddSetCodePoint(ref context, codePoint, ref parser, startIndex);
                        }

                        i = endIndex;
                        break;

                    // CharacterEscape -> RegExpUnicodeEscapeSequence
                    // CharacterEscape -> HexEscapeSequence
                    case 'u':
                    case 'x':
                        if (TryParseHexEscape(pattern, ref i, out charCode))
                        {
                            if (ch == 'u' && char.IsHighSurrogate((char) charCode) && i + 2 < pattern.Length && pattern[i + 1] == '\\' && pattern[i + 2] == 'u')
                            {
                                endIndex = i + 2;
                                if (TryParseHexEscape(pattern, ref endIndex, out charCode2) && char.IsLowSurrogate((char) charCode2))
                                {
                                    codePoint = char.ConvertToUtf32((char) charCode, (char) charCode2);
                                    if (!context.WithinSet)
                                    {
                                        if (sb is not null)
                                        {
                                            AppendCodePointSafe(sb, codePoint);
                                        }

                                        context.FollowingQuantifierError = null;
                                    }
                                    else
                                    {
                                        AddSetCodePoint(ref context, codePoint, ref parser, startIndex);
                                    }

                                    i = endIndex;
                                    break;
                                }
                            }

                            if (!context.WithinSet)
                            {
                                if (sb is not null)
                                {
                                    AppendUnicodeCharSafe(sb, (char) charCode);
                                }

                                context.FollowingQuantifierError = null;
                            }
                            else
                            {
                                AddSetCodePoint(ref context, (char) charCode, ref parser, startIndex);
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError(startIndex, ch == 'u' ? Messages.RegexInvalidUnicodeEscape : Messages.RegexInvalidEscape);
                        }
                        break;

                    // CharacterEscape -> c ControlLetter
                    case 'c':
                        if (i + 1 < pattern.Length)
                        {
                            if (pattern[i + 1] is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
                            {
                                charCode = (ushort) (char.ToUpperInvariant(pattern[++i]) - '@');

                                if (!context.WithinSet)
                                {
                                    context.AppendCharSafe?.Invoke(sb!, (char) charCode);
                                    context.FollowingQuantifierError = null;
                                }
                                else
                                {
                                    AddSetCodePoint(ref context, charCode, ref parser, startIndex);
                                }
                                break;
                            }
                        }

                        parser.ReportSyntaxError(startIndex, Messages.RegexInvalidUnicodeEscape);
                        break;

                    // CharacterEscape -> 0 [lookahead ∉ DecimalDigit] 
                    case '0':
                        if (!Character.IsDecimalDigit(pattern.CharCodeAt(i + 1)))
                        {
                            if (!context.WithinSet)
                            {
                                context.AppendCharSafe?.Invoke(sb!, '\0');
                                context.FollowingQuantifierError = null;
                            }
                            else
                            {
                                AddSetCodePoint(ref context, 0, ref parser, startIndex);
                            }
                        }
                        else
                        {
                            parser.ReportSyntaxError(startIndex, !context.WithinSet
                                ? Messages.RegexInvalidDecimalEscape
                                : Messages.RegexInvalidClassEscape);
                        }
                        break;

                    // DecimalEscape
                    case >= '1' and <= '9':
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

                        // When the number is not a backreference, it's a syntax error.
                        parser.ReportSyntaxError(startIndex, !context.WithinSet || ch >= '8'
                            ? Messages.RegexInvalidEscape
                            : Messages.RegexInvalidClassEscape);
                        break;

                    // 'k' GroupName
                    case 'k':
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
                            // \k escape sequence within character sets is not allowed.
                            parser.ReportSyntaxError(startIndex, Messages.RegexInvalidEscape);
                        }
                        break;

                    // CharacterClassEscape
                    case 'd' or 'D' or 's' or 'S' or 'w' or 'W':
                        if (!context.WithinSet)
                        {
                            // RegexOptions.ECMAScript incorrectly interprets \s as [\f\n\r\t\v\u0020]. This doesn't align with the JS specification,
                            // which defines \s as [\f\n\r\t\v\u0020\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff]. We need to adjust both \s and \S.
                            // \D and \W also have to be adjusted outside character sets.

                            if (sb is not null)
                            {
                                if (ch is 'D' or 'S' or 'W')
                                {
                                    const string InvertedDigitPattern = "\0-\\x2F\\x3A-\uFFFF";
                                    const string InvertedWordCharPattern = "\0-\\x2F\\x3A-\\x40\\x5B-\\x5E\\x60\\x7B-\uFFFF";

                                    // NOTE: It's important to match surrogate pairs with the first alternate to prevent matching the high and low surrogates separately
                                    // (see also https://stackoverflow.com/a/18017758).

                                    sb.Append("(?:");
                                    sb.Append(MatchSurrogatePairRegex);
                                    sb.Append('|').Append('[').Append(ch switch { 'D' => InvertedDigitPattern, 'S' => InvertedWhiteSpacePattern, _ => InvertedWordCharPattern }).Append(']');
                                    sb.Append(')');
                                }
                                else
                                {
                                    _ = ch == 's'
                                        ? sb.Append('[').Append('\\').Append(ch).Append(AdditionalWhiteSpacePattern).Append(']')
                                        : sb.Append(pattern, startIndex, 2);
                                }
                            }

                            context.FollowingQuantifierError = null;
                        }
                        else
                        {
                            if (context.SetRangeStart < 0)
                            {
                                parser.ReportSyntaxError(startIndex, Messages.RegexInvalidCharacterClass);
                            }

                            if (sb is not null)
                            {
                                AddStandardCharClass(ref context.UnicodeSet, ch);
                            }

                            context.SetRangeStart = SetRangeStartedWithCharClass;
                        }
                        break;

                    // CharacterClassEscape -> p{ UnicodePropertyValueExpression }
                    // CharacterClassEscape -> P{ UnicodePropertyValueExpression }
                    case 'p' or 'P':
                        if (pattern.CharCodeAt(i + 1) == '{')
                        {
                            endIndex = pattern.IndexOf('}', i + 2);
                            if (endIndex < 0
                                || (slice = pattern.AsSpan(i + 2, endIndex - (i + 2))).IsWhiteSpace())
                            {
                                parser.ReportSyntaxError(startIndex, !context.WithinSet
                                    ? Messages.RegexInvalidPropertyName
                                    : Messages.RegexInvalidPropertyNameInCharacterClass);
                                slice = default; // keeps the compiler happy
                            }

                            // Unicode property escape support are pretty limited in .NET and we can't even use that little bit
                            // because it only matches characters, not code points (e.g. "\uD80C\uDC00".match(/\p{L}/u) succeeds,
                            // while Regex.Matches("\uD80C\uDC00", @"\p{L}", RegexOptions.ECMAScript) doesn't!)
                            // Until .NET catches up, the only thing we can do is to manually translate property expressions to
                            // code point ranges. However, there are two problems with this:
                            // 1. The resulting set can be huge (there are > 1M code points...)
                            // 2. The Unicode data needed for doing this is too big to include in this project.
                            // So, the best effort we can make ATM is to cook from what .NET provides out of the box,
                            // which is practically General Categories. There's no easy way to support other expressions for now.

                            ArrayList<CodePointRange> categoryRangeList;
                            if (sb is not null)
                            {
                                if (!TryTranslateUnicodePropertyToRanges(slice, parser.GetCodePointRangeCache(), out var categoryRanges))
                                {
                                    parser.ReportConversionFailure(startIndex, "Inconvertible Unicode property escape");
                                    return false;
                                }

                                categoryRangeList = ch == 'P'
                                    ? CodePointRange.InvertRanges(categoryRanges)
                                    : new ArrayList<CodePointRange>(categoryRanges);
                            }
                            else
                            {
                                // NOTE: We skip validating Unicode property expressions because for that we'd need to include a lot of data in the library.

                                categoryRangeList = default;
                            }

                            if (!context.WithinSet)
                            {
                                if (sb is not null)
                                {
                                    AppendSet(sb, categoryRangeList.AsSpan(), isInverted: false);
                                }

                                context.FollowingQuantifierError = null;
                            }
                            else
                            {
                                if (context.SetRangeStart < 0)
                                {
                                    parser.ReportSyntaxError(startIndex, Messages.RegexInvalidCharacterClass);
                                }

                                if (sb is not null)
                                {
                                    context.UnicodeSet.AddRange(categoryRangeList);
                                }

                                context.SetRangeStart = SetRangeStartedWithCharClass;
                            }

                            i = endIndex;
                        }
                        else
                        {
                            parser.ReportSyntaxError(startIndex, !context.WithinSet
                                ? Messages.RegexInvalidPropertyName
                                : Messages.RegexInvalidPropertyNameInCharacterClass);
                        }
                        break;

                    default:
                        if (!TryGetSimpleEscapeCharCode(ch, context.WithinSet, out charCode))
                        {
                            parser.ReportSyntaxError(startIndex, Messages.RegexInvalidEscape);
                        }

                        if (!context.WithinSet)
                        {
                            sb?.Append(pattern, startIndex, 2);
                            context.FollowingQuantifierError = ch is 'b' or 'B' ? Messages.RegexNothingToRepeat : null;
                        }
                        else
                        {
                            AddSetCodePoint(ref context, charCode, ref parser, startIndex);
                        }
                        break;
                }

                return true;
            }
        }
    }
}
