using System.Globalization;
using System.Numerics;

namespace Esprima.Tests;

public class ScannerTests
{
    [Fact]
    public void CanScanMultiLineComment()
    {
        var scanner = new Scanner("var foo=1; /* \"330413500\" */", new ScannerOptions { Comments = true });

        var results = new List<string>();
        Token token;
        do
        {
            foreach (var comment in scanner.ScanComments())
            {
                results.Add($"{comment.Start}-{comment.End}");
            }

            token = scanner.Lex();
        } while (token.Type != TokenType.EOF);

        Assert.Equal(new[] { "11-28" }, results);
    }

    [Fact]
    public void CanResetScanner()
    {
        var scanner = new Scanner("var /* c1 */ foo=1; // c2", new ScannerOptions { Comments = true });

        for (var n = 0; n < 3; n++, scanner.Reset())
        {
            var tokens = new List<Token>();
            var comments = new List<Comment>();
            for (; ; )
            {
                foreach (var comment in scanner.ScanComments())
                {
                    comments.Add(comment);
                }

                var token = scanner.Lex();
                if (token.Type != TokenType.EOF)
                {
                    tokens.Add(token);
                }
                else
                {
                    break;
                }
            }

            Assert.Equal(new object[] { "var", "foo", "=", 1.0, ";" }, tokens.Select(t => t.Value).ToArray());
            Assert.Equal(new string[] { " c1 ", " c2" }, comments.Select(c => scanner.Code.AsSpan(c.Slice.Start, c.Slice.Length).ToString()).ToArray());
        }
    }

    [Fact]
    public void CanResetScannerToCustomPosition()
    {
        var scanner = new Scanner("var /* c1 */ foo=1; // c2", new ScannerOptions { Comments = true });
        scanner.Reset(4, 1, 0);

        var tokens = new List<Token>();
        var comments = new List<Comment>();
        for (; ; )
        {
            foreach (var comment in scanner.ScanComments())
            {
                comments.Add(comment);
            }

            var token = scanner.Lex();
            if (token.Type != TokenType.EOF)
            {
                tokens.Add(token);
            }
            else
            {
                break;
            }
        }

        Assert.Equal(new object[] { "foo", "=", 1.0, ";" }, tokens.Select(t => t.Value).ToArray());
        Assert.Equal(new string[] { " c1 ", " c2" }, comments.Select(c => scanner.Code.AsSpan(c.Slice.Start, c.Slice.Length).ToString()).ToArray());
    }

    [Fact]
    public void ShouldRejectInvalidUnescapedSurrogateAsIdentifierStart()
    {
        // These values are altered by XUnit if passed in InlineData to the test method
        foreach (var s in new[]
        {
            "\ud800",
            "\ud800b",
            "\ud800\ud800",
            "\udc00",
            "\udc00b",
            "\udc00\ud800",
            "\udc00\udc00",
        })
        {
            var scanner = new Scanner(s);
            var ex = Assert.Throws<ParserException>(new Func<object>(() => scanner.Lex()));
            Assert.Equal(Messages.UnexpectedTokenIllegal, ex.Error?.Description);
        }
    }

    [Fact]
    public void ShouldRejectInvalidUnescapedSurrogateAsIdentifierPart()
    {
        // These values are altered by XUnit if passed in InlineData to the test method
        foreach (var s in new[]
        {
            "a\ud800",
            "a\ud800b",
            "a\ud800\ud800",
            "a\udc00",
            "a\udc00b",
            "a\udc00\ud800",
            "a\udc00\udc00",
        })
        {
            var scanner = new Scanner(s);
            var ex = Assert.Throws<ParserException>(new Func<object>(() => scanner.Lex()));
            Assert.Equal(Messages.UnexpectedTokenIllegal, ex.Error?.Description);
        }
    }

    [InlineData(@"\ud800")]
    [InlineData(@"\udc00")]
    [InlineData(@"\ud800\udc00")]
    [InlineData(@"\u{d800}")]
    [InlineData(@"\u{dc00}")]
    [InlineData(@"\u{d800}\u{dc00}")]
    [Theory]
    public void ShouldRejectEscapedSurrogateAsIdentifierStart(string s)
    {
        var scanner = new Scanner(s);
        var ex = Assert.Throws<ParserException>(new Func<object>(() => scanner.Lex()));
        Assert.Equal(Messages.UnexpectedTokenIllegal, ex.Error?.Description);
    }

    [InlineData(@"a\ud800")]
    [InlineData(@"a\udc00")]
    [InlineData(@"a\ud800\udc00")]
    [InlineData(@"a\u{d800}")]
    [InlineData(@"a\u{dc00}")]
    [InlineData(@"a\u{d800}\u{dc00}")]
    [Theory]
    public void ShouldRejectEscapedSurrogateAsIdentifierPart(string s)
    {
        var scanner = new Scanner(s);
        var ex = Assert.Throws<ParserException>(new Func<object>(() => scanner.Lex()));
        Assert.Equal(Messages.UnexpectedTokenIllegal, ex.Error?.Description);
    }

    [Fact]
    public void ShouldAcceptSurrogateRangeInLiterals()
    {
        var scanner = new Scanner(@"'a\u{d800}\u{dc00}'");
        var token = scanner.Lex();
        Assert.Equal(TokenType.StringLiteral, token.Type);
        Assert.Equal("a\ud800\udc00", token.Value);
    }

    private const string ValueOf_255_F_HexDigits = "11,235,582,092,889,474,423,308,157,442,431,404,585,112,356,118,389,416,079,589,380,072,358,292,237,843,810,195,794,279,832,650,471,001,320,007,117,491,962,084,853,674,360,550,901,038,905,802,964,414,967,132,773,610,493,339,054,092,829,768,888,725,077,880,882,465,817,684,505,312,860,552,384,417,646,403,930,092,119,569,408,801,702,322,709,406,917,786,643,639,996,702,871,154,982,269,052,209,770,601,514,008,575";
    private const string ValueOf_256_F_HexDigits = "179,769,313,486,231,590,772,930,519,078,902,473,361,797,697,894,230,657,273,430,081,157,732,675,805,500,963,132,708,477,322,407,536,021,120,113,879,871,393,357,658,789,768,814,416,622,492,847,430,639,474,124,377,767,893,424,865,485,276,302,219,601,246,094,119,453,082,952,085,005,768,838,150,682,342,462,881,473,913,110,540,827,237,163,350,510,684,586,298,239,947,245,938,479,716,304,835,356,329,624,224,137,215";

    [Theory]
    [InlineData("0xfedc_ba98_7654_3210", "18,364,758,544,493,064,720")]
    [InlineData("0xfedc_ba98_7654_3210n", "18,364,758,544,493,064,720")]
    [InlineData("0xFEDC_BA98_7654_3210", "18,364,758,544,493,064,720")]
    [InlineData("0xFEDC_BA98_7654_3210n", "18,364,758,544,493,064,720")]
    [InlineData("0XFEDC_BA98_7654_3210", "18,364,758,544,493,064,720")]
    [InlineData("0XFEDC_BA98_7654_3210n", "18,364,758,544,493,064,720")]
    [InlineData("0o7_7777_7777_7777_7777_7777", "9,223,372,036,854,775,807")]
    [InlineData("0o7_7777_7777_7777_7777_7777n", "9,223,372,036,854,775,807")]
    [InlineData("0O7_7777_7777_7777_7777_7777", "9,223,372,036,854,775,807")]
    [InlineData("0O7_7777_7777_7777_7777_7777n", "9,223,372,036,854,775,807")]
    [InlineData("0b11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000", "18,364,758,544,493,064,720")]
    [InlineData("0b11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000n", "18,364,758,544,493,064,720")]
    [InlineData("0B11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000", "18,364,758,544,493,064,720")]
    [InlineData("0B11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000n", "18,364,758,544,493,064,720")]

    [InlineData("0x1_FEDC_BA98_7654_3210", "36,811,502,618,202,616,336")]
    [InlineData("0x1_FEDC_BA98_7654_3210n", "36,811,502,618,202,616,336")]
    [InlineData("0o37_7334_5651_4166_2503_1020", "36,811,502,618,202,616,336")]
    [InlineData("0o37_7334_5651_4166_2503_1020n", "36,811,502,618,202,616,336")]
    [InlineData("0b1_11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000", "36,811,502,618,202,616,336")]
    [InlineData("0b1_11111110_11011100_10111010_10011000_01110110_01010100_00110010_00010000n", "36,811,502,618,202,616,336")]

    [InlineData("0xFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF", ValueOf_255_F_HexDigits)]
    [InlineData("0xFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFFn", ValueOf_255_F_HexDigits)]
    [InlineData("0xFFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF", "Infinity")]
    [InlineData("0xFFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFFn", ValueOf_256_F_HexDigits)]
    [InlineData("0x0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", "0")] // 256 '0' digits
    public void ShouldCorrectlyParsePrefixedNumber(string input, string expectedValue)
    {
        var scanner = new Scanner(input);

        var token = scanner.Lex();

        var expectedTokenType = input.AsSpan().Last() == 'n' ? TokenType.BigIntLiteral : TokenType.NumericLiteral;
        Assert.Equal(expectedTokenType, token.Type);

        expectedValue = expectedValue.Replace("_", "");

        object expectedValueObj = expectedTokenType == TokenType.NumericLiteral
            ? double.Parse(expectedValue, NumberStyles.AllowThousands, CultureInfo.InvariantCulture)
            : BigInteger.Parse(expectedValue, NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        Assert.Equal(expectedValueObj, token.Value);
    }

    [Theory]
    [InlineData("0", false, "0")]
    [InlineData("0", true, "0")]
    [InlineData("0n", false, "0")]
    [InlineData("0n", true, "0")]
    [InlineData("07", false, "7")]
    [InlineData("07", true, null)]
    [InlineData("07n", false, null)]
    [InlineData("08", false, "8")]
    [InlineData("08", true, null)]
    [InlineData("08n", false, null)]
    [InlineData("017", false, "15")]
    [InlineData("017", true, null)]
    [InlineData("017n", false, null)]
    [InlineData("017.", false, "15")]
    [InlineData("017.", true, null)]
    [InlineData("017.1", false, "15")]
    [InlineData("017.1", true, null)]
    [InlineData("017e", false, null)]
    [InlineData("017e", true, null)]
    [InlineData("018", false, "18")]
    [InlineData("018", true, null)]
    [InlineData("018n", false, null)]
    [InlineData("018.", false, "18")]
    [InlineData("018.", true, null)]
    [InlineData("018.1", false, "18.1")]
    [InlineData("018.1", true, null)]
    [InlineData("018e", false, null)]
    [InlineData("018e", true, null)]
    [InlineData("018.e", false, null)]
    [InlineData("018.e", true, null)]
    [InlineData("018e2", false, "1800")]
    [InlineData("018e2", true, null)]
    [InlineData("018e+", false, null)]
    [InlineData("018e+", true, null)]
    [InlineData("018e+2", false, "1800")]
    [InlineData("018e+2", true, null)]
    [InlineData("018e-", false, null)]
    [InlineData("018e-", true, null)]
    [InlineData("018e-1", false, "1.8")]
    [InlineData("018e-1", true, null)]
    [InlineData("018.1e2", false, "1810")]
    [InlineData("018.1e2", true, null)]
    [InlineData("018.1e+2", false, "1810")]
    [InlineData("018.1e+2", true, null)]
    [InlineData("018.1e-1", false, "1.81")]
    [InlineData("018.1e-1", true, null)]
    [InlineData("7", true, "7")]
    [InlineData("7n", true, "7")]
    [InlineData("17", true, "17")]
    [InlineData("17n", true, "17")]
    [InlineData("17.", true, "17")]
    [InlineData("17.1", true, "17.1")]
    [InlineData("17.1n", true, null)]
    [InlineData("17e", false, null)]
    [InlineData("17.e", false, null)]
    [InlineData("17e2", true, "1700")]
    [InlineData("17e+", false, null)]
    [InlineData("17e+2", true, "1700")]
    [InlineData("17e-", false, null)]
    [InlineData("17e-1", true, "1.7")]
    [InlineData("17.1e2", true, "1710")]
    [InlineData("17.1e+2", true, "1710")]
    [InlineData("17.1e-1", true, "1.71")]
    [InlineData(".7", true, "0.7")]
    [InlineData(".7n", false, null)]
    [InlineData(".17", true, "0.17")]
    [InlineData(".17n", false, null)]
    [InlineData(".17.", true, "0.17")]
    [InlineData(".17.1", true, "0.17")]
    [InlineData(".17e", false, null)]
    [InlineData(".17.e", true, "0.17")]
    [InlineData(".17e2", true, "17")]
    [InlineData(".17e+", false, null)]
    [InlineData(".17e+2", true, "17")]
    [InlineData(".17e-", false, null)]
    [InlineData(".17e-1", true, "0.017")]
    [InlineData(".17.1e2", true, "0.17")]
    [InlineData(".17.1e+2", true, "0.17")]
    [InlineData(".17.1e-1", true, "0.17")]
    [InlineData("0777_7777", false, null)]
    [InlineData("0777_7777", true, null)]

    [InlineData("18_364_758_544_493_064_720", true, "1.8364758544493064e+19")]
    [InlineData("18_364_758_544_493_064_720n", true, "18_364_758_544_493_064_720")]
    [InlineData("36_811_502_618_202_616_336", true, "3.6811502618202616E+19")]
    [InlineData("36_811_502_618_202_616_336n", true, "36,811,502,618,202,616,336")]
    [InlineData("11_235_582_092_889_474_423_308_157_442_431_404_585_112_356_118_389_416_079_589_380_072_358_292_237_843_810_195_794_279_832_650_471_001_320_007_117_491_962_084_853_674_360_550_901_038_905_802_964_414_967_132_773_610_493_339_054_092_829_768_888_725_077_880_882_465_817_684_505_312_860_552_384_417_646_403_930_092_119_569_408_801_702_322_709_406_917_786_643_639_996_702_871_154_982_269_052_209_770_601_514_008_575", true, ValueOf_255_F_HexDigits)]
    [InlineData("11_235_582_092_889_474_423_308_157_442_431_404_585_112_356_118_389_416_079_589_380_072_358_292_237_843_810_195_794_279_832_650_471_001_320_007_117_491_962_084_853_674_360_550_901_038_905_802_964_414_967_132_773_610_493_339_054_092_829_768_888_725_077_880_882_465_817_684_505_312_860_552_384_417_646_403_930_092_119_569_408_801_702_322_709_406_917_786_643_639_996_702_871_154_982_269_052_209_770_601_514_008_575n", true, ValueOf_255_F_HexDigits)]
    [InlineData("179_769_313_486_231_590_772_930_519_078_902_473_361_797_697_894_230_657_273_430_081_157_732_675_805_500_963_132_708_477_322_407_536_021_120_113_879_871_393_357_658_789_768_814_416_622_492_847_430_639_474_124_377_767_893_424_865_485_276_302_219_601_246_094_119_453_082_952_085_005_768_838_150_682_342_462_881_473_913_110_540_827_237_163_350_510_684_586_298_239_947_245_938_479_716_304_835_356_329_624_224_137_215", true, "Infinity")]
    [InlineData("179_769_313_486_231_590_772_930_519_078_902_473_361_797_697_894_230_657_273_430_081_157_732_675_805_500_963_132_708_477_322_407_536_021_120_113_879_871_393_357_658_789_768_814_416_622_492_847_430_639_474_124_377_767_893_424_865_485_276_302_219_601_246_094_119_453_082_952_085_005_768_838_150_682_342_462_881_473_913_110_540_827_237_163_350_510_684_586_298_239_947_245_938_479_716_304_835_356_329_624_224_137_215n", true, ValueOf_256_F_HexDigits)]
    [InlineData("0.000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_01", true, "1e-323")]
    [InlineData("0.000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_001", true, "0")]
    [InlineData(".000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_01", true, "1e-323")]
    [InlineData(".000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_000_001", true, "0")]
    [InlineData("1.8_364_758_544_493_064_720e19", true, "1.8364758544493064e+19")]
    [InlineData("1.8_364_758_544_493_064_720e+19", true, "1.8364758544493064e+19")]
    [InlineData("1_8_364_758_544_493_064_720_000e-3", true, "1.8364758544493064e+19")]
    [InlineData("1_8_364_758_544_493_064_720_000.0e-3", true, "1.8364758544493064e+19")]
    [InlineData("1.8364758544493064720e1_9", true, "1.8364758544493064e+19")]
    [InlineData("1.1235582092889474E+307", true, ValueOf_255_F_HexDigits)]
    [InlineData("1.797_693_134_862_315_9e308", true, "Infinity")]
    [InlineData("1.797_693_134_862_315_9e+308", true, "Infinity")]
    [InlineData("1e-324", true, "0")]
    [InlineData("0.01e-322", true, "0")]
    [InlineData(".01e-322", true, "0")]

    [InlineData("018364758544493064720", false, "1.8364758544493064e+19")]
    [InlineData("018364758544493064720n", false, null)]
    [InlineData("018364758544493064720n", true, null)]
    [InlineData("036811502618202616336", false, "3.6811502618202616E+19")]
    [InlineData("011235582092889474423308157442431404585112356118389416079589380072358292237843810195794279832650471001320007117491962084853674360550901038905802964414967132773610493339054092829768888725077880882465817684505312860552384417646403930092119569408801702322709406917786643639996702871154982269052209770601514008575", false, ValueOf_255_F_HexDigits)]
    [InlineData("0179769313486231590772930519078902473361797697894230657273430081157732675805500963132708477322407536021120113879871393357658789768814416622492847430639474124377767893424865485276302219601246094119453082952085005768838150682342462881473913110540827237163350510684586298239947245938479716304835356329624224137215", false, "Infinity")]
    [InlineData("00.1", false, "0")]
    [InlineData("00.1e-324", false, "0")]
    public void ShouldCorrectlyParseNonPrefixedNumber(string input, bool strict, string? expectedValue)
    {
        var scanner = new Scanner(input);
        var lexOptions = new LexOptions(strict, false);

        if (expectedValue is not null)
        {
            var token = scanner.Lex(lexOptions);

            var expectedTokenType = input.AsSpan().Last() == 'n' ? TokenType.BigIntLiteral : TokenType.NumericLiteral;
            Assert.Equal(expectedTokenType, token.Type);

            expectedValue = expectedValue.Replace("_", "");

            object expectedValueObj = expectedTokenType == TokenType.NumericLiteral
                ? double.Parse(expectedValue, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture)
                : BigInteger.Parse(expectedValue, NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

            Assert.Equal(expectedValueObj, token.Value);
        }
        else
        {
            Assert.Throws<ParserException>(() => scanner.Lex(lexOptions));
        }
    }
}
