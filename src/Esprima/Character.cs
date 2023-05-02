using System.Globalization;
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

    internal static bool IsIdentifierStart(string s, int index)
    {
        return IsIdentifierStartUnicodeCategory(CharUnicodeInfo.GetUnicodeCategory(s, index));
    }

    internal static bool IsIdentifierPart(char ch)
    {
        return (_characterData[ch] & (byte) CharacterMask.IdentifierPart) != 0;
    }

    internal static bool IsIdentifierPart(string s, int index)
    {
        return IsIdentifierPartUnicodeCategory(CharUnicodeInfo.GetUnicodeCategory(s, index));
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

    internal static bool IsIdentifierStartUnicodeCategory(UnicodeCategory cat)
    {
        return IsLetterChar(cat) || cat is UnicodeCategory.ModifierLetter or UnicodeCategory.NonSpacingMark;
    }

    internal static bool IsIdentifierPartUnicodeCategory(UnicodeCategory cat)
    {
        return IsLetterChar(cat)
               || IsDecimalDigitChar(cat)
               || IsConnectingChar(cat)
               || IsCombiningChar(cat)
               || IsFormattingChar(cat);
    }

    internal static bool IsLetterChar(UnicodeCategory cat)
    {
        return cat switch
        {
            UnicodeCategory.UppercaseLetter => true,
            UnicodeCategory.LowercaseLetter => true,
            UnicodeCategory.TitlecaseLetter => true,
            UnicodeCategory.OtherLetter => true,
            UnicodeCategory.LetterNumber => true,
            UnicodeCategory.Surrogate => true,
            UnicodeCategory.OtherNotAssigned => true,
            UnicodeCategory.OtherNumber => true,

            UnicodeCategory.MathSymbol => true,
            UnicodeCategory.OtherSymbol => true,
            UnicodeCategory.ModifierSymbol => true,

            _ => false
        };
    }

    private static bool IsCombiningChar(UnicodeCategory cat)
    {
        return cat is UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark;
    }

    private static bool IsDecimalDigitChar(UnicodeCategory cat)
    {
        return cat == UnicodeCategory.DecimalDigitNumber;
    }

    private static bool IsConnectingChar(UnicodeCategory cat)
    {
        return cat is UnicodeCategory.ConnectorPunctuation or UnicodeCategory.OtherPunctuation;
    }

    internal static bool IsFormattingChar(char ch)
    {
        // There are no FormattingChars in ASCII range
        return ch > 127 && IsFormattingChar(CharUnicodeInfo.GetUnicodeCategory(ch));
    }

    internal static bool IsFormattingChar(UnicodeCategory cat)
    {
        return cat == UnicodeCategory.Format;
    }
}
