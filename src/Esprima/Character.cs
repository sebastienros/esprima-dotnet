using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Esprima;

[Flags]
internal enum CharacterMask : byte
{
    None = 0,
    WhiteSpace = 1,
    IdentifierStart = 2,
    IdentifierPart = 4,
}

public static partial class Character
{
    internal const int UnicodeLastCodePoint = 0x10FFFF;

    // https://tc39.github.io/ecma262/#sec-line-terminators

    internal static bool IsLineTerminator(char ch)
    {
        return ch == 10
               || ch == 13
               || ch == 0x2028 // line separator
               || ch == 0x2029 // paragraph separator
            ;
    }

    // https://tc39.github.io/ecma262/#sec-white-space

    internal static bool IsWhiteSpace(char ch)
    {
        return (_characterData[ch] & (byte) CharacterMask.WhiteSpace) != 0;
    }

    // https://tc39.github.io/ecma262/#sec-names-and-keywords

    internal static bool IsIdentifierStart(char ch)
    {
        return (_characterData[ch] & (byte) CharacterMask.IdentifierStart) != 0;
    }

    internal static bool IsIdentifierStartAstral(int cp)
    {
        Debug.Assert(cp > char.MaxValue);
        return CharRange.CharSetContains(cp, s_identifierStartAstralRanges, s_rangeLengthLookup);
    }

    internal static bool IsIdentifierPart(char ch)
    {
        return (_characterData[ch] & (byte) CharacterMask.IdentifierPart) != 0;
    }

    internal static bool IsIdentifierPartAstral(int cp)
    {
        Debug.Assert(cp > char.MaxValue);
        return CharRange.CharSetContains(cp, s_identifierPartAstralRanges, s_rangeLengthLookup);
    }

    // https://tc39.github.io/ecma262/#sec-literals-numeric-literals

    internal static readonly Func<char, bool> IsDecimalDigitFunc = IsDecimalDigit;

    public static bool IsDecimalDigit(char cp) => IsInRange(cp, '0', '9');

    internal static readonly Func<char, bool> IsHexDigitFunc = IsHexDigit;

    public static bool IsHexDigit(char cp) => HexConverter.IsHexChar(cp);

    internal static readonly Func<char, bool> IsOctalDigitFunc = IsOctalDigit;

    public static bool IsOctalDigit(char cp) => IsInRange(cp, '0', '7');

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsInRange(char c, char min, char max) => c - (uint) min <= max - (uint) min;
}
