using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Esprima;

#if DEBUG
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(), nq}}")]
#endif
internal readonly struct CodePointRange : IComparable<CodePointRange>
{
    public CodePointRange(int codePoint) : this(codePoint, codePoint) { }

    public CodePointRange(int start, int end)
    {
        Debug.Assert(start >= 0 && end <= Character.UnicodeLastCodePoint && start <= end);

        Start = start;
        End = end;
    }

    public readonly int Start;

    public readonly int End; /* Inclusive */

    public int Length => End - Start + 1;

    public bool Contains(int codePoint) => Start <= codePoint && codePoint <= End;

    public int CompareTo(CodePointRange other) => Start - other.Start;

    public static void NormalizeRanges(ref ArrayList<CodePointRange> ranges)
    {
        if (ranges.Count <= 1)
        {
            return;
        }

        // 1. Sort

        ranges.Sort();

        // 2. Optimize

        var optimizedRanges = new ArrayList<CodePointRange>(initialCapacity: ranges.Count);

        var range = ranges[0];
        for (var i = 1; i < ranges.Count; i++)
        {
            if (ranges[i].End <= range.End)
            {
                continue;
            }

            if (range.End >= ranges[i].Start - 1)
            {
                range = new CodePointRange(range.Start, ranges[i].End);
                continue;
            }

            optimizedRanges.Add(range);
            range = ranges[i];
        }
        optimizedRanges.Add(range);

        ranges = optimizedRanges;
    }

    public static ArrayList<CodePointRange> InvertRanges(ReadOnlySpan<CodePointRange> ranges, int start = 0, int end = Character.UnicodeLastCodePoint)
    {
        if (ranges.Length == 0)
        {
            return new ArrayList<CodePointRange>(new[] { new CodePointRange(start, end) });
        }

        var invertedRanges = new ArrayList<CodePointRange>();

        if (ranges[0].Start > start)
        {
            invertedRanges.Add(new CodePointRange(start, ranges[0].Start - 1));
        }

        for (var i = 1; i < ranges.Length; i++)
        {
            invertedRanges.Add(new CodePointRange(ranges[i - 1].End + 1, ranges[i].Start - 1));
        }

        if (ranges.Last().End < end)
        {
            invertedRanges.Add(new CodePointRange(ranges.Last().End + 1, end));
        }

        return invertedRanges;
    }

    internal static bool RangesContain(int codePoint, int[] ranges, int[] rangeLengthLookup)
    {
        Debug.Assert(codePoint is >= 0 and <= Character.UnicodeLastCodePoint);

        var codePointShifted = codePoint << 8;

        var index = Array.BinarySearch(ranges, codePointShifted);
        if (index >= 0
            || (index = ~index) < ranges.Length && DecodeRange(ranges[index], rangeLengthLookup).Contains(codePoint)
            || index > 0 && DecodeRange(ranges[--index], rangeLengthLookup).Contains(codePoint))
        {
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static CodePointRange DecodeRange(int data, int[] rangeLengths)
    {
        var start = data >> 8;
        return new CodePointRange(start, start + rangeLengths[data & 0xFF]);
    }

    internal static void AddRanges(ref ArrayList<CodePointRange> ranges, Predicate<int> includesCodePoint, int start = 0, int end = Character.UnicodeLastCodePoint)
    {
        for (; start <= end; start++)
        {
            var cp = start;

            if (!includesCodePoint(cp))
            {
                continue;
            }

            if (ranges.Count > 0)
            {
                ref var range = ref ranges.AsSpan().Last();
                if (range.End == cp - 1)
                {
                    range = new CodePointRange(range.Start, cp);
                    continue;
                }
            }

            ranges.Add(new CodePointRange(cp));
        }
    }

    public sealed class Cache
    {
        internal static readonly KeyValuePair<int, int> LetterCategory = new((int) UnicodeCategory.OtherNotAssigned + 1,
            1 << (int) UnicodeCategory.UppercaseLetter
            | 1 << (int) UnicodeCategory.LowercaseLetter
            | 1 << (int) UnicodeCategory.TitlecaseLetter
            | 1 << (int) UnicodeCategory.ModifierLetter
            | 1 << (int) UnicodeCategory.OtherLetter);

        internal static readonly KeyValuePair<int, int> CasedLetterCategory = new((int) UnicodeCategory.OtherNotAssigned + 2,
            1 << (int) UnicodeCategory.UppercaseLetter
            | 1 << (int) UnicodeCategory.LowercaseLetter
            | 1 << (int) UnicodeCategory.TitlecaseLetter);

        internal static readonly KeyValuePair<int, int> MarkCategory = new((int) UnicodeCategory.OtherNotAssigned + 3,
            1 << (int) UnicodeCategory.NonSpacingMark
            | 1 << (int) UnicodeCategory.SpacingCombiningMark
            | 1 << (int) UnicodeCategory.EnclosingMark);

        internal static readonly KeyValuePair<int, int> NumberCategory = new((int) UnicodeCategory.OtherNotAssigned + 4,
            1 << (int) UnicodeCategory.DecimalDigitNumber
            | 1 << (int) UnicodeCategory.LetterNumber
            | 1 << (int) UnicodeCategory.OtherNumber);

        internal static readonly KeyValuePair<int, int> PunctuationCategory = new((int) UnicodeCategory.OtherNotAssigned + 5,
            1 << (int) UnicodeCategory.ConnectorPunctuation
            | 1 << (int) UnicodeCategory.DashPunctuation
            | 1 << (int) UnicodeCategory.OpenPunctuation
            | 1 << (int) UnicodeCategory.ClosePunctuation
            | 1 << (int) UnicodeCategory.InitialQuotePunctuation
            | 1 << (int) UnicodeCategory.FinalQuotePunctuation
            | 1 << (int) UnicodeCategory.OtherPunctuation);

        internal static readonly KeyValuePair<int, int> SymbolCategory = new((int) UnicodeCategory.OtherNotAssigned + 6,
            1 << (int) UnicodeCategory.MathSymbol
            | 1 << (int) UnicodeCategory.CurrencySymbol
            | 1 << (int) UnicodeCategory.ModifierSymbol
            | 1 << (int) UnicodeCategory.OtherSymbol);

        internal static readonly KeyValuePair<int, int> SeparatorCategory = new((int) UnicodeCategory.OtherNotAssigned + 7,
            1 << (int) UnicodeCategory.SpaceSeparator
            | 1 << (int) UnicodeCategory.LineSeparator
            | 1 << (int) UnicodeCategory.ParagraphSeparator);

        internal static readonly KeyValuePair<int, int> OtherCategory = new((int) UnicodeCategory.OtherNotAssigned + 8,
            1 << (int) UnicodeCategory.Control
            | 1 << (int) UnicodeCategory.Format
            | 1 << (int) UnicodeCategory.Surrogate
            | 1 << (int) UnicodeCategory.PrivateUse
            | 1 << (int) UnicodeCategory.OtherNotAssigned);

        private readonly CodePointRange[][] _generalCategories = new CodePointRange[OtherCategory.Key + 1][];

        internal CodePointRange[] GetGeneralCategory(UnicodeCategory category)
        {
            return GetGeneralCategory(new KeyValuePair<int, int>((int) category, 1 << (int) category));
        }

        internal CodePointRange[] GetGeneralCategory(KeyValuePair<int, int> category)
        {
            ref var cachedRanges = ref _generalCategories[category.Key];
            if (cachedRanges is null)
            {
                ArrayList<CodePointRange> ranges = default;
                AddRanges(ref ranges, cp => (category.Value & (1 << (int) Shims.GetUnicodeCategory(cp))) != 0);
                ranges.Yield(out cachedRanges, out var count);
                Array.Resize(ref cachedRanges, count);
            }
            return cachedRanges;
        }
    }

#if DEBUG
    private string GetDebuggerDisplay()
    {
        return Start != End
            ? string.Format(CultureInfo.InvariantCulture, "[U+{0:X4}..U+{1:X4}]", Start, End)
            : string.Format(CultureInfo.InvariantCulture, "[U+{0:X4}]", Start);
    }
#endif
}
