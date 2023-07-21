using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Esprima.Utils;

namespace Esprima;

partial class Scanner
{
    [Flags]
    internal enum RegExpFlags
    {
        None = 0,
        Global = 1 << 0,
        Multiline = 1 << 1,
        IgnoreCase = 1 << 2,
        Unicode = 1 << 3,
        Sticky = 1 << 4,
        DotAll = 1 << 5,
        Indices = 1 << 6,
        UnicodeSets = 1 << 7
    }

    internal partial struct RegExpParser
    {
        private const string MatchAnyCharRegex = @"[\s\S]"; // .NET equivalent of /[^]/
        private const string MatchNothingRegex = @"[^\s\S]"; // .NET equivalent of /[]/

        // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/RegExp/dotAll#description
        private const string MatchNewLineRegex = "[\n\r\u2028\u2029]";
        private const string MatchNoNewLineRegex = "[^\n\r\u2028\u2029]";

        // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Regular_expressions/Character_classes#types
        // https://learn.microsoft.com/en-us/dotnet/standard/base-types/character-classes-in-regular-expressions#whitespace-character-s
        private const string AdditionalWhiteSpacePattern = "\u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff";
        private const string InvertedWhiteSpacePattern = "\0-\u0008\u000E-\u001F\\x21-\u009F\u00A1-\u167F\u1681-\u1FFF\u200B-\u2027\u202A-\u202E\u2030-\u205E\u2060-\u2FFF\u3001-\uFEFE\uFF00-\uFFFF";

        private const int SetRangeNotStarted = int.MaxValue;
        private const int SetRangeStartedWithCharClass = int.MaxValue - 1;

        internal static RegExpFlags ParseFlags(string value, int startIndex, Scanner scanner)
        {
            var flags = RegExpFlags.None;

            for (var i = 0; i < value.Length; i++)
            {
                var flag = value[i] switch
                {
                    'g' => RegExpFlags.Global,
                    'i' => RegExpFlags.IgnoreCase,
                    'm' => RegExpFlags.Multiline,
                    'u' => RegExpFlags.Unicode,
                    'y' => RegExpFlags.Sticky,
                    's' => RegExpFlags.DotAll,
                    'd' => RegExpFlags.Indices,
                    'v' => RegExpFlags.UnicodeSets,
                    _ => RegExpFlags.None
                };

                if (flag == RegExpFlags.None || (flags & flag) != 0)
                {
                    // unknown or already set
                    scanner._index = startIndex;
                    scanner.ThrowUnexpectedToken(Messages.InvalidRegExpFlags);
                }
                flags |= flag;
            }

            if ((flags & RegExpFlags.Unicode) != 0 && (flags & RegExpFlags.UnicodeSets) != 0)
            {
                // cannot have them both
                scanner._index = startIndex;
                scanner.ThrowUnexpectedToken(Messages.InvalidRegExpFlags);
            }

            return flags;
        }

        private static RegexOptions FlagsToOptions(RegExpFlags flags)
        {
            // https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options#ecmascript-matching-behavior
            // https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options#compare-using-the-invariant-culture
            var options = RegexOptions.ECMAScript | RegexOptions.CultureInvariant;

            // Flags 's' and 'm' need special care as the equivalent RegexOptions flags have different behavior.

            if ((flags & RegExpFlags.IgnoreCase) != 0)
            {
                // There are subtle differences between the case-insensitive matching behaviors of the JS and .NET regex engines.
                // As a matter of fact, JS uses different algorithms in non-unicode and unicode mode (see https://tc39.es/ecma262/#sec-runtime-semantics-canonicalize-ch)
                // and, unfortunately, .NET matches neither of them. By specifying RegexOptions.CultureInvariant we can approximate the non-unicode behavior to some extent
                // (as it's based on the language-neutral Unicode Default Case Conversion; though it canonicalizes to upper case as opposed to .NET's lower case approach).
                // However, there will still be differences: e. g. "\u2126" (Ω) isn't matched by /[ω]/i while it is by the same pattern in .NET.
                // As for unicode mode, supposedly we have even more differences in behavior (e.g. "ſ" vs. "s").
                // Maybe we could improve the situation by implementing a CultureInfo with a custom TextInfo but that wouldn't be an easy task and
                // probably we would hit a wall anyway as the .NET regex engine seems to do a lot of internal shenanigans around case insensitive matching...

                // https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options#case-insensitive-matching
                options |= RegexOptions.IgnoreCase;
            }

            return options;
        }

        private readonly string _pattern;
        private readonly int _patternStartIndex;
        private readonly RegExpFlags _flags;
        private readonly string _flagsOriginal;
        private readonly Scanner _scanner;
        private StringBuilder? _tempStringBuilder;

        public RegExpParser(string pattern, int patternStartIndex, string flags, int flagsStartIndex, Scanner scanner)
        {
            _pattern = pattern;
            _patternStartIndex = patternStartIndex;
            _flags = ParseFlags(flags, flagsStartIndex, scanner);
            _flagsOriginal = flags;
            _scanner = scanner;
        }

        public RegExpParser(string pattern, string flags, ScannerOptions scannerOptions)
            : this(pattern, patternStartIndex: 0, flags, flagsStartIndex: 0, new Scanner(flags, scannerOptions))
        {
            _scanner.Reset(pattern, null);
        }

        private Scanner MoveScannerTo(int index, out int originalIndex)
        {
            Debug.Assert(0 <= index && index <= _pattern.Length);
            originalIndex = _scanner._index;
            _scanner._index = _patternStartIndex + index;
            return _scanner;
        }

        private Scanner MoveScannerTo(int index) => MoveScannerTo(index, out _);

        private void ReportConversionFailure(int index, string reason)
        {
            MoveScannerTo(index, out var originalIndex)
                .TolerateUnexpectedToken($"Cannot convert regular expression to an equivalent {typeof(Regex)}: {reason}");
            _scanner._index = originalIndex;
        }

        [DoesNotReturn]
        private void ReportSyntaxError(int index, string message)
        {
            MoveScannerTo(index)
                .ThrowUnexpectedToken(string.Format(CultureInfo.InvariantCulture, message, _pattern, _flagsOriginal));
        }

        public Regex? Parse(out string? adaptedPattern)
        {
            if ((_flags & RegExpFlags.UnicodeSets) != 0)
            {
                const string UnicodeSetsModeNotSupported = "Unicode sets mode (flag v) is not supported currently";
                if (_scanner._regExpParseMode is RegExpParseMode.AdaptToInterpreted or RegExpParseMode.AdaptToCompiled)
                {
                    ReportConversionFailure(0, UnicodeSetsModeNotSupported);
                }
                else
                {
                    MoveScannerTo(0).ThrowUnexpectedToken(UnicodeSetsModeNotSupported);
                }

                adaptedPattern = null;
                return null;
            }

            adaptedPattern = ParseCore();
            if (adaptedPattern is null)
            {
                return null;
            }

            var options = FlagsToOptions(_flags);
            if (_scanner._regExpParseMode == RegExpParseMode.AdaptToCompiled)
            {
                options |= RegexOptions.Compiled;
            }
            var matchTimeout = _scanner._regexTimeout;

            try
            {
                return new Regex(adaptedPattern, options, matchTimeout);
            }
            catch
            {
                ReportConversionFailure(0, "Failed to adapt regular expression");
                return null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal string? ParseCore()
        {
            CheckBracesBalance(out var capturingGroups, out var capturingGroupNames);

            return (_flags & RegExpFlags.Unicode) != 0
                ? ParsePattern(UnicodeMode.Instance, ref capturingGroups, capturingGroupNames)
                : ParsePattern(LegacyMode.Instance, ref capturingGroups, capturingGroupNames);
        }

        /// <summary>
        /// Ensures the braces are balanced in the regular expression pattern.
        /// </summary>
        private void CheckBracesBalance(out ArrayList<RegExpCapturingGroup> capturingGroups, out Dictionary<string, string?>? capturingGroupNames)
        {
            capturingGroups = default;
            capturingGroupNames = null;

            var isUnicode = (_flags & RegExpFlags.Unicode) != 0;
            var inGroup = 0;
            var inQuantifier = false;
            var inSet = false;

            // Potential problematic constructs:
            // * Escaped opening/closing brackets (\(, \), \[, \], \{, \}, \<, \>) --> These are handled (see below).
            // * ?<Name> and \k<Name> --> Shouldn't be an actual problem as opening/closing brackets are not allowed to occur in capturing group names.
            // * \p{...} --> Might be problematic as, in theory, property values can contain special chars (see https://unicode.org/reports/tr18/#property_syntax),
            //   however it seems that currently no such value is defined (see https://unicode.org/Public/UCD/latest/ucd/PropertyValueAliases.txt),
            //   so we can ignore this for now.

            for (var i = 0; i < _pattern.Length; i++)
            {
                var ch = _pattern[i];

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

                        var groupType = DetermineGroupType(i);
                        if (groupType == RegExpGroupType.Capturing)
                        {
                            capturingGroups.Add(new RegExpCapturingGroup(i, Name: null));
                        }
                        else if (groupType == RegExpGroupType.NamedCapturing)
                        {
                            var startIndex = i++;
                            if (ParseNormalizedCapturingGroupName(ref i) is { } groupName)
                            {
                                (capturingGroupNames ??= new Dictionary<string, string?>()).TryAdd(groupName, null);
                                capturingGroups.Add(new RegExpCapturingGroup(i, groupName));
                            }
                            else
                            {
                                ReportSyntaxError(startIndex + 3, Messages.RegExpInvalidCaptureGroupName);
                            }
                        }
                        else if (groupType == RegExpGroupType.Unknown)
                        {
                            ReportSyntaxError(i, Messages.RegExpInvalidGroup);
                        }

                        break;

                    case ')':

                        if (inSet)
                        {
                            break;
                        }

                        if (inGroup == 0)
                        {
                            ReportSyntaxError(i, Messages.RegExpUnmatchedOpenParen);
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
                        else if (isUnicode)
                        {
                            ReportSyntaxError(i, Messages.RegExpIncompleteQuantifier);
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
                        else if (isUnicode)
                        {
                            ReportSyntaxError(i, Messages.RegExpLoneQuantifierBrackets);
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
                        else if (isUnicode)
                        {
                            ReportSyntaxError(i, Messages.RegExpLoneQuantifierBrackets);
                        }

                        break;

                    default: break;
                }
            }

            if (inGroup > 0)
            {
                ReportSyntaxError(_pattern.Length, Messages.RegExpUnterminatedGroup);
            }

            if (inSet)
            {
                ReportSyntaxError(_pattern.Length, Messages.RegExpUnterminatedCharacterClass);
            }

            if (isUnicode)
            {
                if (inQuantifier)
                {
                    ReportSyntaxError(_pattern.Length, Messages.RegExpLoneQuantifierBrackets);
                }
            }
        }

        /// <summary>
        /// Check the regular expression pattern for additional syntax errors and optionally build an adjusted pattern which
        /// implements the equivalent behavior in .NET, on top of the <see cref="RegexOptions.ECMAScript"/> compatibility mode.
        /// </summary>
        /// <returns>
        /// <see langword="null"/> if the scanner is configured to validate the regular expression pattern but not adapt it to .NET.
        /// Otherwise, the adapted pattern or <see langword="null"/> if the pattern is syntactically correct but a .NET equivalent could not be constructed
        /// and the scanner is configured to tolerant mode.
        /// </returns>
        private string? ParsePattern<TMode>(TMode mode, ref ArrayList<RegExpCapturingGroup> capturingGroups, Dictionary<string, string?>? capturingGroupNames)
            where TMode : IMode
        {
            StringBuilder? sb;
            if (_scanner._regExpParseMode is RegExpParseMode.AdaptToInterpreted or RegExpParseMode.AdaptToCompiled)
            {
                sb = _scanner.GetStringBuilder();
                if (sb.Capacity < _pattern.Length)
                {
                    sb.Capacity = _pattern.Length;
                }
            }
            else
            {
                sb = null;
            }

            var context = new ParsePatternContext(sb, capturingGroups.AsSpan(), capturingGroupNames)
            {
                CapturingGroupCounter = 0,
                GroupStack = capturingGroupNames is not null
                    ? new ArrayList<RegExpGroup>(new[] { new RegExpGroup(default, parent: null) })
                    : default,
                SetStartIndex = -1,
                FollowingQuantifierError = Messages.RegExpNothingToRepeat,
            };

            ref var i = ref context.Index;
            for (i = 0; i < _pattern.Length; i++)
            {
                var ch = _pattern[i];
                switch (ch)
                {
                    case '[' when !context.WithinSet:
                        context.SetStartIndex = i;
                        context.SetRangeStart = SetRangeNotStarted;

                        mode.ProcessSetSpecialChar(ref context, ch);

                        if ((ch = _pattern.CharCodeAt(i + 1)) == '^')
                        {
                            mode.ProcessSetSpecialChar(ref context, ch);
                            i++;
                        }
                        break;

                    case '-' when context.WithinSet:
                        if (context.SetRangeStart >= 0 && context.SetRangeStart != SetRangeNotStarted)
                        {
                            // We use bitwise complement to indicate that '-' was encountered after a character (or character class like \d or \p{...}).
                            context.SetRangeStart = ~context.SetRangeStart;
                            mode.ProcessSetSpecialChar(ref context, ch);
                        }
                        else
                        {
                            // We encountered a case like /[-]/, /[0-9-]/, /[0-/d-]/, /[/d-0-]/ or /[\0--]/
                            mode.ProcessSetChar(ref context, ch, context.AppendCharSafe, ref this, startIndex: i);
                        }
                        break;

                    case ']':
                        if (!context.WithinSet)
                        {
                            Debug.Assert(mode is LegacyMode, Messages.RegExpLoneQuantifierBrackets); // CheckBracesBalance should ensure this.
                            goto default;
                        }

                        if (!mode.RewriteSet(ref context, ref this))
                        {
                            mode.ProcessSetSpecialChar(ref context, ch);
                        }

                        context.SetStartIndex = -1;
                        context.FollowingQuantifierError = null;
                        break;

                    case '(' when !context.WithinSet:
                        var currentGroupAlternate = capturingGroupNames is not null
                            ? context.GroupStack.AsSpan().Last().LastAlternate
                            : null;

                        var groupType = DetermineGroupType(i);
                        if (groupType == RegExpGroupType.Capturing)
                        {
                            context.CapturingGroupCounter++;
                        }
                        else if (groupType == RegExpGroupType.NamedCapturing)
                        {
                            var groupName = capturingGroups[context.CapturingGroupCounter++].Name;
                            Debug.Assert(groupName is not null);

                            if (!currentGroupAlternate!.TryAddGroupName(groupName!))
                            {
                                ReportSyntaxError(i + 3, Messages.RegExpDuplicateCaptureGroupName);
                            }

                            if (sb is not null)
                            {
                                groupName = AdjustCapturingGroupName(groupName!, capturingGroupNames!);
                                if (groupName is null)
                                {
                                    ReportConversionFailure(i + 3, $"Cannot map group name '{groupName}' to a unique group name in the adapted regex.");
                                    return null;
                                }

                                sb.Append(_pattern, i, 3).Append(groupName);
                            }

                            i = _pattern.IndexOf('>', i + 3);
                            Debug.Assert(i >= 0);
                            sb?.Append(_pattern[i]);

                            context.GroupStack.Push(new RegExpGroup(groupType, currentGroupAlternate));
                            context.FollowingQuantifierError = Messages.RegExpNothingToRepeat;
                            break;
                        }

                        sb?.Append(_pattern, i, 1 + ((int) groupType >> 2));
                        i += (int) groupType >> 2;

                        context.GroupStack.Push(currentGroupAlternate is not null
                            ? new RegExpGroup(groupType, currentGroupAlternate)
                            : new RegExpGroup(groupType));
                        context.FollowingQuantifierError = Messages.RegExpNothingToRepeat;
                        break;

                    case '|' when !context.WithinSet:
                        sb?.Append(ch);

                        if (capturingGroupNames is not null)
                        {
                            context.GroupStack.AsSpan().Last().AddAlternate();
                        }
                        context.FollowingQuantifierError = Messages.RegExpNothingToRepeat;
                        break;

                    case ')' when !context.WithinSet:
                        Debug.Assert(context.GroupStack.Count > (capturingGroupNames is not null ? 1 : 0), Messages.RegExpUnmatchedOpenParen); // CheckBracesBalance should ensure this.

                        if (capturingGroupNames is not null)
                        {
                            context.GroupStack.AsSpan().Last().HoistGroupNamesToParent();
                        }

                        groupType = context.GroupStack.Pop().Type;

                        sb?.Append(ch);

                        context.FollowingQuantifierError = mode.AllowsQuantifierAfterGroup(groupType) ? null : Messages.RegExpInvalidQuantifier;
                        break;

                    // RegexOptions.Multiline matches only '\n' and has other behavioral differences (e.g. "a\r\n\b".match(/^$/m) matches,
                    // while Regex.Matches("a\r\n\b", @"^$", RegexOptions.ECMAScript | RegexOptions.Multiline) doesn't!)
                    // We can simulate this using RegexOptions.ECMAScript (without RegexOptions.Multiline) + positive lookbehind/lookahead.

                    case '^' when !context.WithinSet:
                        if (sb is not null)
                        {
                            _ = (_flags & RegExpFlags.Multiline) != 0
                                ? sb.Append("(?<=").Append(MatchNewLineRegex).Append('|').Append(ch).Append(')')
                                : sb.Append(ch);
                        }

                        context.FollowingQuantifierError = Messages.RegExpNothingToRepeat;
                        break;

                    case '$' when !context.WithinSet:
                        if (sb is not null)
                        {
                            _ = (_flags & RegExpFlags.Multiline) != 0
                                ? sb.Append("(?=").Append(MatchNewLineRegex).Append('|').Append(ch).Append(')')
                                : sb.Append(ch);
                        }

                        context.FollowingQuantifierError = Messages.RegExpNothingToRepeat;
                        break;

                    case '.' when !context.WithinSet:
                        // The behavior of /./ depends on multiple flags:
                        // * Flag 's' determines whether to match new line characters or not (see https://github.com/tc39/proposal-regexp-dotall-flag).
                        //   We need to rewrite dots even in the latter case because RegexOptions.ECMAScript doesn't handle them correctly as
                        //   it only treats '\n' as new line while JS treats a few other characters like that as well.
                        // * Flag 'u' also changes the behavior (it must match code points instead of characters).
                        mode.RewriteDot(ref context, (_flags & RegExpFlags.DotAll) != 0);

                        context.FollowingQuantifierError = null;
                        break;

                    case '*' or '+' or '?' when !context.WithinSet:
                        if (context.FollowingQuantifierError is not null)
                        {
                            ReportSyntaxError(i, context.FollowingQuantifierError);
                        }

                        sb?.Append(ch);

                        if ((ch = _pattern.CharCodeAt(i + 1)) == '?')
                        {
                            sb?.Append(ch);
                            i++;
                        }

                        context.FollowingQuantifierError = Messages.RegExpNothingToRepeat;
                        break;

                    case '{' when !context.WithinSet:
                        if (!TryAdjustRangeQuantifier(ref context))
                        {
                            mode.HandleInvalidRangeQuantifier(ref context, ref this, i);
                            break;
                        }
                        else if (i < 0) // conversion is impossible
                        {
                            return null;
                        }

                        if ((ch = _pattern.CharCodeAt(i + 1)) == '?')
                        {
                            sb?.Append(ch);
                            i++;
                        }

                        context.FollowingQuantifierError = Messages.RegExpNothingToRepeat;
                        break;

                    case '\\':
                        Debug.Assert(i + 1 < _pattern.Length, "Unexpected end of escape sequence in regular expression.");
                        if (!mode.AdjustEscapeSequence(ref context, ref this))
                        {
                            return null;
                        }
                        break;

                    default:
                        if (!context.WithinSet)
                        {
                            mode.ProcessChar(ref context, ch, context.AppendChar, ref this);
                            context.FollowingQuantifierError = null;
                        }
                        else
                        {
                            mode.ProcessSetChar(ref context, ch,
                                !(ch == '[' && context.SetRangeStart < 0) ? context.AppendChar : context.AppendCharSafe,
                                ref this, startIndex: i);
                        }
                        break;
                }
            }

            return sb?.ToString();
        }

        private static readonly Action<StringBuilder, char> s_appendChar = static (sb, ch) => sb.Append(ch);
        private static readonly Action<StringBuilder, char> s_appendCharSafe = AppendCharSafe;

        private static void AppendCharSafe(StringBuilder sb, char ch)
        {
            // We don't unescape character code sequences in the printable ASCII character range (U+0020..U+007E) to
            // prevent problems which could arise in the case of special regex characters.
            // (This could be further optimized though by unescaping + escaping the problematic characters with '\'.)

            _ = (ushort) ch is >= 0x20 and < 0x7F
                ? sb.Append('\\').Append('x').Append(((byte) ch).ToString("X2", CultureInfo.InvariantCulture))
                : sb.Append(ch);
        }

        private static bool TryParseHexEscape(string pattern, ref int i, out ushort charCode)
        {
            var charCodeLength = pattern[i] == 'u' ? 4 : 2;

            if (i + charCodeLength < pattern.Length)
            {
                if (ushort.TryParse(pattern.AsSpan(i + 1, charCodeLength).ToParsable(), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out charCode))
                {
                    i += charCodeLength;
                    return true;
                }
            }

            charCode = default;
            return false;
        }

        private static bool TryGetSimpleEscapeCharCode(char ch, bool withinSet, out ushort charCode)
        {
            switch (ch)
            {
                // Assertion (word boundary) / Backspace 
                case 'b':
                    // NOTE: For the sake of simplicity, we also use this logic for validation in unicode mode,
                    // so we return an unused dummy value for word boundary escapes outside character sets.
                    charCode = withinSet ? '\b' : char.MaxValue;
                    return true;
                case 'B':
                    charCode = char.MaxValue;
                    return !withinSet;

                // CharacterEscape -> ControlEscape
                case 'f': charCode = '\f'; return true;
                case 'n': charCode = '\n'; return true;
                case 't': charCode = '\t'; return true;
                case 'r': charCode = '\r'; return true;
                case 'v': charCode = '\v'; return true;

                // CharacterEscape -> IdentityEscape -> '/'
                case '/':

                // CharacterEscape -> IdentityEscape -> SyntaxCharacter
                case '^' or '$' or '\\' or '.' or '*' or '+' or '?' or '(' or ')' or '[' or ']' or '{' or '}' or '|':
                    charCode = ch;
                    return true;

                // '-' is not a SyntaxCharacter by definition but must be escaped in character sets.
                // (However, outside the class it is not allowed to be escaped in unicode mode!)
                case '-':
                    charCode = ch;
                    return withinSet;
            }

            charCode = default;
            return false;
        }

        private bool TryAdjustRangeQuantifier(ref ParsePatternContext context)
        {
            ref readonly var sb = ref context.StringBuilder;
            ref var i = ref context.Index;

            var endIndex = _pattern.IndexOf('}', i + 1);
            if (endIndex < 0 || endIndex == i + 1)
            {
                return false;
            }

            var index = _pattern.IndexOf(',', i + 1, endIndex - (i + 1));
            if (index < 0)
            {
                index = endIndex;
            }

            int min, max;
            var slice = _pattern.AsSpan(i + 1, index - (i + 1));
            if (!int.TryParse(slice.ToParsable(), NumberStyles.None, CultureInfo.InvariantCulture, out min))
            {
                if (slice.Length == 0 || slice.FindIndex(ch => !Character.IsDecimalDigit(ch)) >= 0)
                {
                    return false;
                }
                min = -1;
            }

            if (index == endIndex)
            {
                max = min;
            }
            else if (index == endIndex - 1)
            {
                max = int.MaxValue;
            }
            else
            {
                slice = _pattern.AsSpan(index + 1, endIndex - (index + 1));
                if (!int.TryParse(slice.ToParsable(), NumberStyles.None, CultureInfo.InvariantCulture, out max))
                {
                    if (slice.FindIndex(ch => !Character.IsDecimalDigit(ch)) >= 0)
                    {
                        min = max = default;
                        return false;
                    }
                    max = -1;
                }
            }

            if (min >= 0 && max >= 0)
            {
                if (min > max)
                {
                    ReportSyntaxError(i, Messages.RegExpNumbersOutOfOrderInQuantifier);
                }

                if (context.FollowingQuantifierError is not null)
                {
                    ReportSyntaxError(i, context.FollowingQuantifierError);
                }

                sb?.Append(_pattern, i, endIndex + 1 - i);
            }
            else if (sb is not null)
            {
                // According to the spec (https://tc39.es/ecma262/#sec-patterns-static-semantics-early-errors),
                // number of occurrences can be an arbitrarily big number, however implementations (incl. V8) seems to ignore numbers greater than int.MaxValue.
                // (e.g. /x{2147483647,2147483646}/ is syntax error while /x{2147483648,2147483647}/ is not!)
                // We report failure in this case because .NET regex engine doesn't allow numbers greater than int.MaxValue.
                ReportConversionFailure(i, "Inconvertible {} quantifier");
                i = -1;
                return true;
            }

            i = endIndex;
            return true;
        }

        private RegExpGroupType DetermineGroupType(int i)
        {
            if (++i >= _pattern.Length || _pattern[i] != '?')
            {
                return RegExpGroupType.Capturing;
            }

            if (++i >= _pattern.Length)
            {
                return RegExpGroupType.Unknown;
            }

            return _pattern[i] switch
            {
                ':' => RegExpGroupType.NonCapturing,
                '=' => RegExpGroupType.LookaheadAssertion,
                '!' => RegExpGroupType.NegativeLookaheadAssertion,
                '<' => (++i < _pattern.Length ? _pattern[i] : char.MinValue) switch
                {
                    '=' => RegExpGroupType.LookbehindAssertion,
                    '!' => RegExpGroupType.NegativeLookbehindAssertion,
                    _ => RegExpGroupType.NamedCapturing,
                },
                _ => RegExpGroupType.Unknown
            };
        }

        private string? ParseNormalizedCapturingGroupName(ref int i)
        {
            if (_pattern.CharCodeAt(i + 1) == '<')
            {
                var endIndex = _pattern.IndexOf('>', i + 2);
                if (endIndex < 0 || endIndex == i + 2 || !Character.IsIdentifierStart(_pattern[i + 2]))
                {
                    return null;
                }

                MoveScannerTo(i + 2, out var originalIndex);
                var originalLength = _scanner._length;
                var originalStringBuilder = _scanner._sb;
                _scanner._length = _patternStartIndex + endIndex;
                _scanner._sb = _tempStringBuilder?.Clear() ?? (_tempStringBuilder = new StringBuilder());
                if (_tempStringBuilder.Capacity < _scanner._length - _scanner._index)
                {
                    _tempStringBuilder.Capacity = _scanner._length - _scanner._index;
                }

                string? name;
                try
                {
                    name = (ushort) _scanner._source[_scanner._index] is 0x5C or (>= 0xD800 and <= 0xDFFF)
                        ? _scanner.GetComplexIdentifier(allowEscapedSurrogates: true)
                        : _scanner.GetIdentifier(allowEscapedSurrogates: true);

                    if (_scanner.Eof())
                    {
                        i = endIndex;
                    }
                    else
                    {
                        name = null;
                    }
                }
                catch (ParserException)
                {
                    name = null;
                }

                _scanner._index = originalIndex;
                _scanner._length = originalLength;
                _scanner._sb = originalStringBuilder;

                return name;
            }

            return null;
        }

        private static string? AdjustCapturingGroupName(string groupName, Dictionary<string, string?> capturingGroupNames)
        {
            // 0. Check that the adjusted name is already available.

            var adjustedGroupName = capturingGroupNames[groupName];
            if (adjustedGroupName is not null)
            {
                return adjustedGroupName;
            }

            // .NET capture group names can't start with a decimal digit (luckily, JS capture names can't either) and
            // can only contain characters defined by the IsWordChar method
            // (see also https://github.com/dotnet/runtime/blob/v6.0.16/src/libraries/System.Text.RegularExpressions/src/System/Text/RegularExpressions/RegexParser.cs#L868)

            // 1. When the group name contains invalid characters, rewrite it to a string which is a valid group name in .NET and can be reversed

            if (groupName.FindIndex(ch => !IsWordChar(ch)) < 0)
            {
                capturingGroupNames[groupName] = groupName;
                return groupName;
            }

            adjustedGroupName = EncodeGroupName(groupName);

            // 2. Check that the adjusted group name is unique.

            if (!capturingGroupNames.ContainsKey(adjustedGroupName))
            {
                capturingGroupNames[groupName] = adjustedGroupName;
                return adjustedGroupName;
            }

            return null;

            static bool IsWordChar(char ch)
            {
                // Source: https://github.com/dotnet/runtime/blob/v6.0.16/src/libraries/System.Text.RegularExpressions/src/System/Text/RegularExpressions/RegexCharClass.cs#L918

                // According to UTS#18 Unicode Regular Expressions (http://www.unicode.org/reports/tr18/)
                // RL 1.4 Simple Word Boundaries  The class of <word_character> includes all Alphabetic
                // values from the Unicode character database, from UnicodeData.txt [UData], plus the U+200C
                // ZERO WIDTH NON-JOINER and U+200D ZERO WIDTH JOINER.

                // 16 bytes, representing the chars 0 through 127, with a 1 for a bit where that char is a word char
                static ReadOnlySpan<byte> AsciiLookup() => new byte[]
                {
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x03,
                    0xFE, 0xFF, 0xFF, 0x87, 0xFE, 0xFF, 0xFF, 0x07
                };

                // Fast lookup in our lookup table for ASCII characters.  This is purely an optimization, and has the
                // behavior as if we fell through to the switch below (which was actually used to produce the lookup table).
                ReadOnlySpan<byte> asciiLookup = AsciiLookup();
                int chDiv8 = ch >> 3;
                if ((uint) chDiv8 < asciiLookup.Length)
                {
                    return (asciiLookup[chDiv8] & (1 << (ch & 0x7))) != 0;
                }

                // For non-ASCII, fall back to checking the Unicode category.
                switch (CharUnicodeInfo.GetUnicodeCategory(ch))
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.ConnectorPunctuation:
                        return true;

                    default:
                        const char ZeroWidthJoiner = '\u200D', ZeroWidthNonJoiner = '\u200C';
                        return ch == ZeroWidthJoiner || ch == ZeroWidthNonJoiner;
                }
            }
        }

        internal static string EncodeGroupName(string groupName)
        {
            return "__utf8_" + BitConverter.ToString(Encoding.UTF8.GetBytes(groupName)).Replace("-", "");
        }

        private bool TryAdjustBackreference(ref ParsePatternContext context, int startIndex)
        {
            ref readonly var sb = ref context.StringBuilder;
            ref var i = ref context.Index;

            var endIndex = _pattern.FindIndex(ch => !Character.IsDecimalDigit(ch), startIndex: i + 1);
            if (endIndex < 0)
            {
                endIndex = _pattern.Length;
            }

            var slice = _pattern.AsSpan(i, endIndex - i);
            var number = int.Parse(slice.ToParsable(), NumberStyles.None, CultureInfo.InvariantCulture);
            if (number > context.CapturingGroups.Length)
            {
                return false;
            }

            if (startIndex > context.CapturingGroups[number - 1].StartIndex)
            {
                sb?.Append(_pattern, startIndex, endIndex - startIndex);
            }
            else if (sb is not null)
            {
                // RegexOptions.ECMAScript treats forward references like /\1(A)/ differently than JS,
                // so we don't make an attempt at rewriting them.
                ReportConversionFailure(startIndex, "Inconvertible forward reference");
                i = -1;
                return true;
            }

            i = endIndex - 1;
            return true;
        }

        private void AdjustNamedBackreference(ref ParsePatternContext context, int startIndex)
        {
            ref readonly var sb = ref context.StringBuilder;
            ref var i = ref context.Index;

            // 'k' GroupName
            if (ParseNormalizedCapturingGroupName(ref i) is { } groupName)
            {
                if (context.CapturingGroupNames?.TryGetValue(groupName, out var adjustedGroupName) is true)
                {
                    if (sb is not null)
                    {
                        if (IsDefinedCapturingGroupName(groupName, startIndex, context.CapturingGroups))
                        {
                            sb.Append(_pattern, startIndex, 3).Append(adjustedGroupName).Append(_pattern[i]);
                        }
                        else
                        {
                            // RegexOptions.ECMAScript treats forward references like /\k<a>(?<a>A)/ differently than JS,
                            // so we don't make an attempt at rewriting them.
                            ReportConversionFailure(startIndex, "Inconvertible named forward reference");
                            i = -1;
                        }
                    }
                }
                else
                {
                    ReportSyntaxError(startIndex, Messages.RegExpInvalidNamedCaptureReferenced);
                }
            }
            else if (_pattern.CharCodeAt(i + 1) == '<')
            {
                ReportSyntaxError(startIndex + 3, Messages.RegExpInvalidCaptureGroupName);
            }
            else
            {
                ReportSyntaxError(startIndex + 2, Messages.RegExpInvalidNamedReference);
            }
        }

        private static bool IsDefinedCapturingGroupName(string value, int startIndex, ReadOnlySpan<RegExpCapturingGroup> capturingGroups)
        {
            for (var i = 0; i < capturingGroups.Length; i++)
            {
                var group = capturingGroups[i];
                if (group.StartIndex < startIndex && group.Name == value)
                {
                    return true;
                }
            }
            return false;
        }

        private CodePointRange.Cache GetCodePointRangeCache()
        {
            return _scanner._codePointRangeCache ??= new CodePointRange.Cache();
        }

        private ref struct ParsePatternContext
        {
            public ParsePatternContext(StringBuilder? sb, ReadOnlySpan<RegExpCapturingGroup> capturingGroups, Dictionary<string, string?>? capturingGroupNames)
            {
                StringBuilder = sb;
                if (sb is not null)
                {
                    AppendChar = s_appendChar;
                    AppendCharSafe = s_appendCharSafe;
                }

                CapturingGroups = capturingGroups;
                CapturingGroupNames = capturingGroupNames;
            }

            public int Index;

            public readonly StringBuilder? StringBuilder;
            public readonly Action<StringBuilder, char>? AppendChar;
            public readonly Action<StringBuilder, char>? AppendCharSafe;

            public readonly ReadOnlySpan<RegExpCapturingGroup> CapturingGroups;

            public readonly Dictionary<string, string?>? CapturingGroupNames;

            // The number of capturing groups encountered so far. Will be increased when the opening bracket of a capturing group is found.
            public int CapturingGroupCounter;

            // Originally, group names are unique in JS regexes but there's a proposal which may change this soon
            // (see https://github.com/tc39/proposal-duplicate-named-capturing-groups).
            // The .NET regex engine handles duplicate group names fine, so nothing prevents us from implementing this,
            // however it makes things a bit more complicated: group names have still to be unique in an alternate part of a group,
            // so we need to do some extra bookkeeping to handle this.
            public ArrayList<RegExpGroup> GroupStack;

            // The start index of a character set (e.g. /[a-z]/). Negative values indicate that the parser is not within a character set currently.
            public int SetStartIndex;

            public bool WithinSet { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => SetStartIndex >= 0; }

            // A variable which keeps track of ranges in character sets and encodes multiple pieces of information related to this:
            // Basically, it stores the starting code point of a potential range. However, it can also have the following special values:
            // * SetRangeNotStarted - Indicates that a range hasn't started yet (i.e. we're at the start of the set or right after a range).
            // * SetRangeStartedWithCharClass - Indicates that a potential invalid range has started with a character class like \d or \p{...} (e.g. /[\d-A]/).
            // May store the bitwise complement of the possible values listed above, which indicates that the range indicator '-' has been encountered.
            public int SetRangeStart;

            // A variable which keeps track whether the current construct can be followed by a quantifier. A null value indicates that a quantifier can follow,
            // otherwise it stores the error message for cases where a quantifier follows.
            public string? FollowingQuantifierError;

            // We have extra problems in unicode mode:

            // \u{1F4A9} == 💩 == \ud83d\udca9
            // \u{1F4AB} == 💫 == \ud83d\udcab

            // .NET's Regex only looks at single System.Char units. U+1F4A9 for example is two Chars that, from Regex's perspective, are independent.
            // "[💩-💫]" is "[\ud83d\udca9-\ud83d\udcab]", so it just looks at the individual Char values, it sees "\udca9-\ud83d", which is not ordered, hence the error.
            // This is a known design / limitation of Regex that's existed since it was added, and there are currently no plans to improve that.
            // The Regex needs to be rewritten to (?:\ud83d[\udca9-\udcab])

            // To sum it up, in unicode mode we need to completely rewrite character sets as follows:
            // * Surrogate pairs should match the code point and not the high or low part of the surrogate pair.
            // * Inverted character sets should include all Unicode characters (including the U+10000..U+10FFFF range) except the specified characters.
            // * Ranges where the start or end is a surrogate pair need special care.
            // * Lone surrogates need special care too.
            // We use the following list to build the adjusted character set.
            public ArrayList<CodePointRange> UnicodeSet;
        }

        private interface IMode
        {
            void ProcessChar(ref ParsePatternContext context, char ch, Action<StringBuilder, char>? appender, ref RegExpParser parser);

            void ProcessSetSpecialChar(ref ParsePatternContext context, char ch);

            void ProcessSetChar(ref ParsePatternContext context, char ch, Action<StringBuilder, char>? appender, ref RegExpParser parser, int startIndex);

            bool RewriteSet(ref ParsePatternContext context, ref RegExpParser parser);

            void RewriteDot(ref ParsePatternContext context, bool dotAll);

            bool AllowsQuantifierAfterGroup(RegExpGroupType groupType);

            void HandleInvalidRangeQuantifier(ref ParsePatternContext context, ref RegExpParser parser, int startIndex);

            bool AdjustEscapeSequence(ref ParsePatternContext context, ref RegExpParser parser);
        }
    }

    // Enum values encodes the length of the group prefix so that (value / 4) is equal to prefix length.
    private enum RegExpGroupType
    {
        Unknown,
        Capturing = 0 * 4 + 1, // (x)
        NamedCapturing = 2 * 4 + 0, // (?<Name>x)
        NonCapturing = 2 * 4 + 1, // (?:x)
        LookaheadAssertion = 2 * 4 + 2, // x(?=y)
        NegativeLookaheadAssertion = 2 * 4 + 3, // x(?!y)
        LookbehindAssertion = 3 * 4 + 0, // (?<=y)x
        NegativeLookbehindAssertion = 3 * 4 + 1, // (?<!y)x
    }

    private struct RegExpGroup
    {
        // NOTE: Instead of just using a list, we can leverage our economic internal data structure to optimize the case of no alternates.
        private AdditionalDataSlot _alternates;

        public RegExpGroup(RegExpGroupType type)
        {
            Type = type;
        }

        public RegExpGroup(RegExpGroupType type, RegExpGroupAlternate? parent) : this(type)
        {
            _alternates.PrimaryData = new RegExpGroupAlternate(parent);
            AlternateCount = 1;
        }

        public RegExpGroupType Type { get; }

        public int AlternateCount { get; private set; }

        public RegExpGroupAlternate? FirstAlternate => (RegExpGroupAlternate?) _alternates.PrimaryData;

        public RegExpGroupAlternate? LastAlternate => GetAlternate(AlternateCount - 1);

        public RegExpGroupAlternate? GetAlternate(int index) => (RegExpGroupAlternate?) _alternates[index];

        public void AddAlternate()
        {
            Debug.Assert(FirstAlternate is not null);
            _alternates[AlternateCount++] = new RegExpGroupAlternate(FirstAlternate!.Parent);
        }

        public void HoistGroupNamesToParent()
        {
            Debug.Assert(FirstAlternate is not null);
            var parent = FirstAlternate!.Parent;
            Debug.Assert(parent is not null);

            for (var i = 0; i < AlternateCount; i++)
            {
                GetAlternate(i)!.HoistGroupNamesTo(parent!);
            }
        }
    }

    private sealed class RegExpGroupAlternate
    {
        private ArrayList<string> _groupNames;

        public RegExpGroupAlternate(RegExpGroupAlternate? parent)
        {
            Parent = parent;
        }

        public readonly RegExpGroupAlternate? Parent;

        public bool IsDefinedGroupName(string value) => _groupNames.AsSpan().BinarySearch(value) >= 0;

        public bool TryAddGroupName(string value)
        {
            var index = _groupNames.AsSpan().BinarySearch(value);

            var isDefined = index >= 0;
            var scope = this;
            for (; ; )
            {
                if (isDefined)
                {
                    return false;
                }

                if ((scope = scope!.Parent) is null)
                {
                    break;
                }

                isDefined = scope.IsDefinedGroupName(value);
            }

            _groupNames.Insert(~index, value);
            return true;
        }

        public void HoistGroupNamesTo(RegExpGroupAlternate other)
        {
            if (_groupNames.Count > 0)
            {
                if (other._groupNames.Count == 0)
                {
                    other._groupNames = _groupNames;
                }
                else
                {
                    foreach (var groupName in _groupNames)
                    {
                        var index = other._groupNames.AsSpan().BinarySearch(groupName);
                        if (index < 0)
                        {
                            other._groupNames.Insert(~index, groupName);
                        }
                    }
                }
                _groupNames = default;
            }
        }
    }

    private readonly record struct RegExpCapturingGroup(int StartIndex, string? Name);
}
