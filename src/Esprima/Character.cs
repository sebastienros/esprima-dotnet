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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasCharacterFlag(char ch, CharacterMask flag)
    {
        return (s_characterData[ch >> 1] & ((byte) flag << ((ch & 1) << 2))) != 0;
    }

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
        return HasCharacterFlag(ch, CharacterMask.WhiteSpace);
    }

    // https://tc39.github.io/ecma262/#sec-names-and-keywords

    internal static bool IsIdentifierStart(char ch)
    {
        return HasCharacterFlag(ch, CharacterMask.IdentifierStart);
    }

    internal static bool IsIdentifierStartAstral(int cp)
    {
        Debug.Assert(cp > char.MaxValue);
        return CodePointRange.RangesContain(cp, s_identifierStartAstralRanges, s_rangeLengthLookup);
    }

    internal static bool IsIdentifierPart(char ch)
    {
        return HasCharacterFlag(ch, CharacterMask.IdentifierPart);
    }

    internal static bool IsIdentifierPartAstral(int cp)
    {
        Debug.Assert(cp > char.MaxValue);
        return CodePointRange.RangesContain(cp, s_identifierPartAstralRanges, s_rangeLengthLookup);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void GetSurrogatePair(uint cp, out char highSurrogate, out char lowSurrogate)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v6.0.16/src/libraries/System.Private.CoreLib/src/System/Text/UnicodeUtility.cs#L58

        Debug.Assert(cp is > char.MaxValue and <= Character.UnicodeLastCodePoint);

        highSurrogate = (char) ((cp + ((0xD800u - 0x40u) << 10)) >> 10);
        lowSurrogate = (char) ((cp & 0x3FFu) + 0xDC00u);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint GetCodePoint(char highSurrogate, char lowSurrogate)
    {
        // Based on: https://github.com/dotnet/runtime/blob/v6.0.16/src/libraries/System.Private.CoreLib/src/System/Text/UnicodeUtility.cs#L28

        return ((uint) highSurrogate << 10) + lowSurrogate - ((0xD800U << 10) + 0xDC00U - (1 << 16));
    }
}
