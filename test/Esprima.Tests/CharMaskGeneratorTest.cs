#if NET6_0_OR_GREATER

using System.Globalization;
using System.Text;
using System.Unicode;

namespace Esprima.Tests;

/// <summary>
/// Helper to generate some character lookup data.
/// </summary>
public class CharMaskGeneratorTest
{
    [Fact]
    public void UnicodeCharacterUtilitiesWorks()
    {
        Assert.False(UnicodeCharacterUtilities.IsIdentifierPart((char) 65279));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierPart((char) 8204));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierPart('\u0061'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart('\u309B'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart('\u1885'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierPart('゛'));
        Assert.False(UnicodeCharacterUtilities.IsIdentifierStart('\u2E2F'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierPart('\u00B7'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart('ࠚ'));

        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart('$'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart('_'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart('\\'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierPart('$'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierPart('_'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierPart('\\'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart('\u2118'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierPart('\u212E'));

        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart('ࠚ'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart('ꭩ'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart('ꚜ'));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart((char) 55305));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart('\u0870'));

        Assert.False(UnicodeCharacterUtilities.IsIdentifierPart('\u2E2F'));
        Assert.False(UnicodeCharacterUtilities.IsIdentifierStart('\u2E2F'));

        Assert.True(UnicodeCharacterUtilities.IsIdentifierStart(0x13000));
        Assert.False(UnicodeCharacterUtilities.IsIdentifierStart(0x1F4A9));
        Assert.True(UnicodeCharacterUtilities.IsIdentifierPart(0x13000));
        Assert.False(UnicodeCharacterUtilities.IsIdentifierPart(0x1F4A9));
    }

    [Fact]
    public void LookupWorks()
    {
        foreach (var (actual, expectedBmp, expectedAstral) in new[]
        {
            (new Predicate<int>(UnicodeCharacterUtilities.IsWhiteSpace), new Predicate<char>(Character.IsWhiteSpace), new Predicate<int>(_ => false)),
            (UnicodeCharacterUtilities.IsIdentifierStart, Character.IsIdentifierStart, Character.IsIdentifierStartAstral),
            (UnicodeCharacterUtilities.IsIdentifierPart, Character.IsIdentifierPart, Character.IsIdentifierPartAstral),
        })
        {
            for (int ch = char.MinValue; ch <= char.MaxValue; ch++)
            {
                Assert.Equal(actual(ch), expectedBmp((char) ch));
            }

            for (var cp = char.MaxValue + 1; cp < Character.UnicodeLastCodePoint; cp++)
            {
                Assert.Equal(actual(cp), expectedAstral(cp));
            }
        }
    }

    [Fact]
    public void CanGenerateMasks()
    {
        var sb = new StringBuilder();

        sb.AppendLine("namespace Esprima;");
        sb.AppendLine();

        sb.AppendLine($"// Generated using {nameof(CharMaskGeneratorTest)}.{nameof(CanGenerateMasks)}");
        sb.AppendLine("partial class Character");
        sb.AppendLine("{");

        GenerateBmpMasks(sb);

        sb.AppendLine();

        GenerateAstralRanges(sb);

        sb.AppendLine("}");

        // because xunit if what is it, take it from debugger...
        var result = sb.ToString();
    }

    private static void GenerateBmpMasks(StringBuilder sb)
    {
        var masks = new byte[(char.MaxValue + 1) / 2];
        for (int c = char.MinValue; c <= char.MaxValue; ++c)
        {
            var mask = CharacterMask.None;
            if (UnicodeCharacterUtilities.IsWhiteSpace(c))
            {
                mask |= CharacterMask.WhiteSpace;
            }
            if (UnicodeCharacterUtilities.IsIdentifierStart(c))
            {
                mask |= CharacterMask.IdentifierStart;
            }
            if (UnicodeCharacterUtilities.IsIdentifierPart(c))
            {
                mask |= CharacterMask.IdentifierPart;
            }

            masks[c >> 1] |= (byte) ((byte) mask << ((c & 1) << 2));
        }

        sb.AppendLine("    private static ReadOnlySpan<byte> s_characterData => new byte[]");
        sb.AppendLine("    {");
        foreach (var chunk in masks.Chunk(32))
        {
            sb.Append("        ");
            foreach (var value in chunk)
            {
                sb.Append("0x").Append(value.ToString("X2"));
                sb.Append(", ");
            }

            sb.AppendLine();
        }

        sb.AppendLine("    };");
    }

    private static void GenerateAstralRanges(StringBuilder sb)
    {
        var lengthLookup = new List<int> { 0 };

        foreach (var (match, name) in new[]
        {
            (new Predicate<int>(UnicodeCharacterUtilities.IsWhiteSpace), "whiteSpace"),
            (UnicodeCharacterUtilities.IsIdentifierStart, "identifierStart"),
            (UnicodeCharacterUtilities.IsIdentifierPart, "identifierPart"),
        })
        {
            var ranges = new ArrayList<CodePointRange>();

            CodePointRange.AddRanges(ref ranges, match, start: 0x10000);

            if (ranges.Count == 0)
            {
                continue;
            }

            sb.AppendLine($"    private static readonly int[] s_{name}AstralRanges = new[]");
            sb.AppendLine("    {");

            foreach (var chunk in ranges.Chunk(16))
            {
                sb.Append("        ");
                foreach (var range in chunk)
                {
                    sb.Append("0x").Append(EncodeRange(range, lengthLookup).ToString("X8", CultureInfo.InvariantCulture));
                    sb.Append(", ");
                }

                sb.AppendLine();
            }

            sb.AppendLine("    };");
            sb.AppendLine();
        }

        sb.AppendLine($"    private static readonly int[] s_rangeLengthLookup = new int[]");
        sb.AppendLine("    {");
        foreach (var chunk in lengthLookup.Chunk(40))
        {
            sb.Append("        ");
            foreach (var length in chunk)
            {
                sb.Append(length.ToString(CultureInfo.InvariantCulture));
                sb.Append(", ");
            }

            sb.AppendLine();
        }
        sb.AppendLine("    };");
    }

    private static int EncodeRange(CodePointRange range, List<int> lengthLookup)
    {
        var lengthMinusOne = range.End - range.Start;
        var lengthLookupIndex = lengthLookup.IndexOf(lengthMinusOne);
        if (lengthLookupIndex == -1)
        {
            lengthLookupIndex = lengthLookup.Count;
            Assert.True(lengthLookup.Count < byte.MaxValue);
            lengthLookup.Add(lengthMinusOne);
        }
        return range.Start << 8 | checked((byte) lengthLookupIndex);
    }
}

internal static class UnicodeCharacterUtilities
{
    public static bool IsWhiteSpace(int cp)
    {
        // https://tc39.es/ecma262/#prod-WhiteSpace

        return cp is
            '\t' /* <TAB> */
            or '\v' /* <VT> */
            or '\f' /* <FF> */
            or '\uFEFF' /* <ZWNBSP> */
            || UnicodeInfo.GetCharInfo(cp).Category == UnicodeCategory.SpaceSeparator; // <USP>
    }

    public static bool IsIdentifierStart(int cp)
    {
        // https://tc39.es/ecma262/#prod-IdentifierStartChar

        return cp is '$' or '_'
            || UnicodeInfo.GetCharInfo(cp).CoreProperties.HasFlag(CoreProperties.IdentifierStart) /* UnicodeIDStart */
            // Although these chars are not in the spec, we include them and leave up to the scanner to deal with them.
            || cp is '\\' or (>= 0xD800 and <= 0xDFFF);
    }

    public static bool IsIdentifierPart(int cp)
    {
        // https://tc39.es/ecma262/#prod-IdentifierPartChar

        return cp is '$'
            or '\u200C' // <ZWNJ>
            or '\u200D' // <ZWJ>
            || UnicodeInfo.GetCharInfo(cp).CoreProperties.HasFlag(CoreProperties.IdentifierContinue) /* UnicodeIDContinue */
            // Although these chars are not in the spec, we include them and leave up to the scanner to deal with them.
            || cp is '\\' or (>= 0xD800 and <= 0xDFFF);
    }
}

#endif
