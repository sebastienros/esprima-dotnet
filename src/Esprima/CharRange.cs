using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Esprima;

internal readonly partial struct CharRange
{
    public CharRange(int start, int end)
    {
        Start = start;
        End = end;
    }

    public readonly int Start;

    public readonly int End; /* Inclusive */

    public int Length => End - Start + 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(int codePoint) => Start <= codePoint && codePoint <= End;

    internal static bool CharSetContains(int codePoint, int[] charSet, int[] rangeLengthLookup)
    {
        Debug.Assert(codePoint is >= 0 and <= Character.UnicodeLastCodePoint);

        var codePointShifted = codePoint << 8;

        var index = Array.BinarySearch(charSet, codePointShifted);
        if (index >= 0
            || (index = ~index) < charSet.Length && DecodeCharRange(charSet[index], rangeLengthLookup).Contains(codePoint)
            || index > 0 && DecodeCharRange(charSet[--index], rangeLengthLookup).Contains(codePoint))
        {
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static CharRange DecodeCharRange(int data, int[] rangeLengths)
    {
        var start = data >> 8;
        return new CharRange(start, start + rangeLengths[data & 0xFF]);
    }
}
