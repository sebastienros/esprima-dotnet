using System.Text;
using System.Text.RegularExpressions;
using Esprima.Ast;
using Esprima.Test;
using Esprima.Tests.Helpers;

namespace Esprima.Tests;

public class RegExpTests
{
    [Fact]
    public void DotAll()
    {
        Assert.Matches(CreateRegex("/^.$/s"), "\n");
        Assert.DoesNotMatch(CreateRegex("/^.$/"), "\n");
    }

    private static Regex CreateRegex(string code)
    {
        var options = new ScannerOptions { RegExpParseMode = RegExpParseMode.AdaptToInterpreted };
        var token = new Scanner(code, options).ScanRegExp();
        return (Regex) token.Value!;
    }

    [Theory]
    [InlineData(@"/[^]*?(:[rp][el]a[\w-]+)[^]*/")]
    [InlineData(@"/[^]/")]
    [InlineData(@"/[^ ]/")]
    [InlineData(@"/[]/")]
    [InlineData(@"/[]*/")]
    [InlineData(@"/[]a/")]
    public void ShouldParseRegularExpression(string regexp)
    {
        var parser = new JavaScriptParser();
        var program = parser.ParseScript(@"var O = " + regexp);

        Assert.NotNull(program);
    }

    [Fact]
    public void ShouldParseUnicodeEscapesWithoutFlag()
    {
        Assert.Matches(CreateRegex(@"/^\u{3}$/"), "uuu");
        Assert.Matches(CreateRegex(@"/^\\u{3}$/"), @"\uuu");
    }

    [Fact]
    public void ShouldEscapeUnicodeEscapesWithoutFlag()
    {
        Assert.Matches(CreateRegex(@"/^\\u{3}$/"), @"\uuu");
        Assert.Matches(CreateRegex(@"/^\\\u{3}$/"), @"\uuu");
        Assert.Matches(CreateRegex(@"/^\\\\u{3}$/"), @"\\uuu");
        Assert.Matches(CreateRegex(@"/^\\\\\u{3}$/"), @"\\uuu");
    }

    [Fact]
    public void ShouldParseUnicodeEscapesWithFlag()
    {
        Assert.Matches(CreateRegex(@"/^\u{1F680}$/u"), "🚀");
    }

    [Fact]
    public void ShouldParseSurrogatePairs()
    {
        Assert.Matches(CreateRegex(@"/^\uD83D\uDE80$/u"), "🚀");
    }

    [Fact]
    public void ShouldMatchPoop()
    {
        var regex = CreateRegex(@"/foo(.)bar/u");
        var matches = regex.Matches("foo💩bar");
        Assert.Equal("💩", matches[0].Groups[1].Value);
    }

    [Theory]
    [InlineData("a", "(?:[\\x61])")]
    [InlineData("α", "(?:[\u03B1])")]
    [InlineData("💩", "(?:💩)")]
    public void ShouldConvertSetsToUnicode(string source, string expected)
    {
        Assert.Equal(expected, SerializeSet(source));

        string? SerializeSet(string expression)
        {
            var parser = new Scanner.RegExpParser($"[{expression}]", "u", ScannerOptions.Default);
            return parser.ParseCore(out _);
        }
    }

    [Fact]
    public void ShouldConvertSetsToUnicodeSpecial()
    {
        // These values are altered by XUnit if passed in InlineData to ShouldConvertSetsToUnicode
        Assert.Equal("(?:\ud83d[\udca9-\udcab])", SerializeSet("💩-💫"));
        Assert.Equal("(?:[\ud800-\ud83c][\udc00-\udfff]|\ud83d[\udc00-\udca9]|[\ud800-\udbff](?![\udc00-\udfff])|(?<![\ud800-\udbff])[\udc00-\udfff]|[\\x61-\ud7ff-\uffff])", SerializeSet("a-💩"));

        string? SerializeSet(string expression)
        {
            var parser = new Scanner.RegExpParser($"[{expression}]", "u", ScannerOptions.Default);
            return parser.ParseCore(out _);
        }
    }

    [Fact]
    public void ShouldMatchSymbols()
    {

        // Regex only looks at single System.Char units. U+1F4A9 for example is two Chars that, from Regex 's perspective, are independent.
        // "[💩-💫]" is "[\ud83d\udca9-\ud83d\udcab]", so it just looks at the individual Char values, it sees "\udca9-\ud83d", which is not ordered, hence the error.
        // This is a known design / limitation of Regex that's existed since it was added, and there are currently no plans to improve that.
        // The Regex needs to be rewritten to (?:\ud83d[\udca9-\udcab])

        var regex1 = CreateRegex(@"/[\u{1F4A9}-\u{1F4AB}]/u");
        Assert.DoesNotMatch(regex1, "a");
        Assert.Matches(regex1, "💩");
        Assert.Matches(regex1, "💪");
        Assert.Matches(regex1, "💫");
        Assert.DoesNotMatch(regex1, "💬");

        var regex2 = CreateRegex(@"/[💩-💫]/u");
        Assert.DoesNotMatch(regex2, "a");
        Assert.Matches(regex2, "💩");
        Assert.Matches(regex2, "💪");
        Assert.Matches(regex2, "💫");
        Assert.DoesNotMatch(regex2, "💬");
    }

    [Fact]
    public void ShouldNotAcceptOctalEspacesWithUnicodeFlag()
    {
        Assert.Throws<ParserException>(() => CreateRegex(@"/\1/u"));
        Assert.Throws<ParserException>(() => CreateRegex(@"/\251/u"));
        Assert.Throws<ParserException>(() => CreateRegex(@"/\00/u"));
        Assert.NotNull(CreateRegex(@"/\0/u")); // NULL == \u0000
        Assert.NotNull(CreateRegex(@"/\1/"));
    }

    [Theory]
    [InlineData(@"/(/")]
    [InlineData(@"/)/")]
    [InlineData(@"/[/")]
    [InlineData(@"/(/u")]
    [InlineData(@"/)/u")]
    [InlineData(@"/[/u")]
    [InlineData(@"/]/u")]
    [InlineData(@"/{/u")]
    [InlineData(@"/}/u")]
    public void ShouldFailGroupBalance(string pattern)
    {
        Assert.Throws<ParserException>(() => CreateRegex(pattern));
    }

    [Theory]
    [InlineData(@"/]/")]
    [InlineData(@"/{/")]
    [InlineData(@"/}/")]
    [InlineData(@"/[(]/")]
    [InlineData(@"/[)]/")]
    [InlineData(@"/[{]/")]
    [InlineData(@"/[}]/")]
    [InlineData(@"/[[]/")]
    [InlineData(@"/([-.*+?^${}()|[\]\/\\])/")]
    [InlineData(@"/^(?:]|})/")]
    [InlineData(@"/[a-z]/")]
    [InlineData(@"/[a-z]/u")]
    public void ShouldCheckGroupBalance(string pattern)
    {
        Assert.NotNull(CreateRegex(pattern));
    }

    [Fact]
    public void ShouldPreventInfiniteLoopWhenAdaptingMultiLine()
    {
        var regex = Scanner.AdaptRegExp("\\$", "gm", matchTimeout: TimeSpan.FromSeconds(10)).Regex;
        Assert.NotNull(regex);
    }

    [Fact]
    public void ShouldParseBundleJs()
    {
        // It contains very diverse unicode regular expression

        var path = Path.Combine(Fixtures.GetFixturesPath(), "Fixtures", "3rdparty", "bundle.js");
        var source = File.ReadAllText(path);
        var parser = new JavaScriptParser(new ParserOptions { RegExpParseMode = RegExpParseMode.AdaptToInterpreted });
        parser.ParseScript(source);
    }

    [InlineData("/ab?c/u", RegExpParseMode.Skip, false, false, null)]
    [InlineData("/ab?c/u", RegExpParseMode.Skip, true, false, null)]
    [InlineData("/ab?c/u", RegExpParseMode.Validate, false, false, null)]
    [InlineData("/ab?c/u", RegExpParseMode.Validate, true, false, null)]
    [InlineData("/ab?c/u", RegExpParseMode.AdaptToInterpreted, false, false, false)]
    [InlineData("/ab?c/u", RegExpParseMode.AdaptToInterpreted, true, false, false)]
    [InlineData("/ab?c/u", RegExpParseMode.AdaptToCompiled, false, false, true)]
    [InlineData("/ab?c/u", RegExpParseMode.AdaptToCompiled, true, false, true)]
    [InlineData("/ab|?c/u", RegExpParseMode.Skip, false, false, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.Skip, true, false, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.Validate, false, true, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.Validate, true, true, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.AdaptToInterpreted, false, true, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.AdaptToInterpreted, true, true, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.AdaptToCompiled, false, true, null)]
    [InlineData("/ab|?c/u", RegExpParseMode.AdaptToCompiled, true, true, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.Skip, false, false, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.Skip, true, false, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.Validate, false, false, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.Validate, true, false, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.AdaptToInterpreted, false, true, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.AdaptToInterpreted, true, false, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.AdaptToCompiled, false, true, null)]
    [InlineData("/\\1a(b?)c/u", RegExpParseMode.AdaptToCompiled, true, false, null)]
    [Theory]
    public void ShouldRespectParserOptions(string expression, RegExpParseMode parseMode, bool tolerant, bool expectError, bool? expectCompiled)
    {
        var matchTimeout = TimeSpan.FromMilliseconds(1234);

        var parser = new JavaScriptParser(new ParserOptions { RegExpParseMode = parseMode, RegexTimeout = matchTimeout, Tolerant = tolerant });

        if (!expectError)
        {
            var expr = parser.ParseExpression(expression);

            Assert.IsType<RegExpLiteral>(expr);

            var regex = expr.As<RegExpLiteral>().RegexValue;
            if (expectCompiled is not null)
            {
                Assert.NotNull(regex);
                Assert.Equal(expectCompiled.Value, regex.Options.HasFlag(RegexOptions.Compiled));
                Assert.Equal(matchTimeout, regex.MatchTimeout);
            }
            else
            {
                Assert.Null(regex);
            }
        }
        else
        {
            Assert.Throws<ParserException>(() => parser.ParseExpression(expression));
        }
    }

    [InlineData("ab?c", "u", false, false, false, false)]
    [InlineData("ab?c", "u", false, true, false, false)]
    [InlineData("ab?c", "u", true, false, false, true)]
    [InlineData("ab?c", "u", true, true, false, true)]
    [InlineData("ab|?c", "u", false, false, true, null)]
    [InlineData("ab|?c", "u", false, true, true, null)]
    [InlineData("ab|?c", "u", true, false, true, null)]
    [InlineData("ab|?c", "u", true, true, true, null)]
    [InlineData("\\1a(b?)c", "u", false, false, false, null)]
    [InlineData("\\1a(b?)c", "u", false, true, true, null)]
    [InlineData("\\1a(b?)c", "u", true, false, false, null)]
    [InlineData("\\1a(b?)c", "u", true, true, true, null)]
    [Theory]
    public void AdaptRegExpShouldRespectParameters(string pattern, string flags, bool compiled, bool throwIfNotAdaptable, bool expectError, bool? expectCompiled)
    {
        var matchTimeout = TimeSpan.FromMilliseconds(1234);

        if (!expectError)
        {
            var regex = Scanner.AdaptRegExp(pattern, flags, compiled, matchTimeout, throwIfNotAdaptable).Regex;
            if (expectCompiled is not null)
            {
                Assert.NotNull(regex);
                Assert.Equal(expectCompiled.Value, regex.Options.HasFlag(RegexOptions.Compiled));
                Assert.Equal(matchTimeout, regex.MatchTimeout);
            }
            else
            {
                Assert.Null(regex);
            }
        }
        else
        {
            Assert.Throws<ParserException>(() => Scanner.AdaptRegExp(pattern, flags, compiled, matchTimeout, throwIfNotAdaptable));
        }
    }

    [InlineData("ab?c", "u", true)]
    [InlineData("ab|?c", "u", false)]
    [InlineData("\\1a(b?)c", "u", true)]
    [Theory]
    public void ValidateRegExpShouldWork(string pattern, string flags, bool expectedResult)
    {
        Assert.Equal(expectedResult, Scanner.ValidateRegExp(pattern, flags, out var error));
        if (expectedResult)
        {
            Assert.Null(error);
        }
        else
        {
            Assert.NotNull(error);
        }
    }

    [InlineData("(?<a>x)|(?<a>y)", "u", "(?<a>x)|(?<a>y)")]
    [InlineData("((?<a>x))|(?<a>y)", "u", "((?<a>x))|(?<a>y)")]
    [InlineData("(?:(?<a>x))|(?<a>y)", "u", "(?:(?<a>x))|(?<a>y)")]
    [InlineData("(?<!(?<a>x))|(?<a>y)", "u", "(?<!(?<a>x))|(?<a>y)")]
    [InlineData("(?<a>x)|((?<a>y))", "u", "(?<a>x)|((?<a>y))")]
    [InlineData("(?<a>x)|(?:(?<a>y))", "u", "(?<a>x)|(?:(?<a>y))")]
    [InlineData("(?<a>x)|(?!(?<a>y))", "u", "(?<a>x)|(?!(?<a>y))")]
    [InlineData("(?<a>x)|(?<a>y)|(?<a>z)", "u", "(?<a>x)|(?<a>y)|(?<a>z)")]
    [InlineData("((?<a>x)|(?<a>y))|(?<a>z)", "u", "((?<a>x)|(?<a>y))|(?<a>z)")]
    [InlineData("(?<a>x)|((?<a>y)|(?<a>z))", "u", "(?<a>x)|((?<a>y)|(?<a>z))")]
    [InlineData("(?<a>x)|(((?<a>y)))|(?<a>z)", "u", "(?<a>x)|(((?<a>y)))|(?<a>z)")]
    [Theory]
    public void ShouldAllowDuplicateGroupNamesInAlternates(string pattern, string flags, string expectedAdaptedPattern)
    {
        // TODO: Generate these tests when Duplicate named capturing groups (https://github.com/tc39/proposal-duplicate-named-capturing-groups) gets implemented in V8.

        var parser = new Scanner.RegExpParser(pattern, flags, new ScannerOptions { Tolerant = false });
        var actualAdaptedPattern = parser.ParseCore(out _);

        Assert.Equal(expectedAdaptedPattern, actualAdaptedPattern);
    }

    public static IEnumerable<object[]> TestCases(string relativePath)
    {
        var fixturesPath = Path.Combine(Fixtures.GetFixturesPath(), relativePath);
        var testCasesFilePath = Path.Combine(fixturesPath, "testcases.txt");

        if (!File.Exists(testCasesFilePath))
        {
            yield break;
        }

        using var reader = new StreamReader(testCasesFilePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var parts = line.Split(new[] { '\t' }, StringSplitOptions.None);
            if (parts.Length >= 5)
            {
                Array.Resize(ref parts, 6);

                var hints = parts[parts.Length - 1];

                var hintArray = !string.IsNullOrEmpty(hints)
                    ? hints.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    : Array.Empty<string>();

                if (hintArray.Contains("!skip"))
                {
                    continue;
                }

#if NET462_OR_GREATER
                if (hintArray.Contains("!skip-on-netframework"))
                {
                    continue;
                }
#endif
            }

            yield return parts;
        }
    }

    [Theory]
    [MemberData(nameof(TestCases), "Fixtures.RegExp")]
    public void ExecuteTestCase(string pattern, string flags, string expectedAdaptedPattern, string testString, string expectedMatchesJson, string hints)
    {
        // To re-generate test cases, execute `dotnet run --project Fixtures.RegExp\Generator -c Release`

        static string DecodeStringIfEscaped(string value) => JavaScriptStringHelper.IsStringLiteral(value)
            ? JavaScriptStringHelper.Decode(value)
            : value;

        pattern = DecodeStringIfEscaped(pattern);
        flags = DecodeStringIfEscaped(flags);
        expectedAdaptedPattern = DecodeStringIfEscaped(expectedAdaptedPattern);
        testString = DecodeStringIfEscaped(testString);
        var hintArray = !string.IsNullOrEmpty(hints)
            ? hints.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            : Array.Empty<string>();

        // Both Newtonsoft.Json and System.Text.Json mess up lone surrogates,
        // so we need to parse the JSON containing the matches "manually"...
        var (expectedMatches, syntaxError) = RegExpMatch.MatchesFrom(JavaScriptStringHelper.ParseAsExpression(expectedMatchesJson));

        var regexValidator = new Scanner.RegExpParser(pattern, flags, new ScannerOptions { RegExpParseMode = RegExpParseMode.Validate, Tolerant = false });
        var regexConverter = new Scanner.RegExpParser(pattern, flags, new ScannerOptions { RegExpParseMode = RegExpParseMode.AdaptToInterpreted, Tolerant = expectedMatches is not null });

        if (expectedMatches is not null)
        {
            var parseResult = regexValidator.Parse();
            Assert.True(parseResult.Success);
            Assert.Null(parseResult.Regex);
            Assert.Null(parseResult.ConversionError);

            parseResult = regexConverter.Parse();
            if (expectedAdaptedPattern != ")inconvertible(")
            {
                Assert.True(parseResult.Success);
                Assert.NotNull(parseResult.Regex);

                var actualAdaptedPattern = parseResult.Regex.ToString();
                Assert.Equal(expectedAdaptedPattern, actualAdaptedPattern);

                var actualMatchEnumerable = parseResult.Regex.Matches(testString).Cast<Match>();

                // In unicode mode, we can't prevent empty matches within surrogate pairs currently,
                // so we need to remove such matches from the match collection to make assertions pass.
                var actualMatches = flags.IndexOf('u') >= 0
                    ? actualMatchEnumerable
                        .Cast<Match>()
                        .Where(m => m.Length != 0
                            || m.Index == 0 || m.Index == testString.Length
                            || !(char.IsHighSurrogate(testString[m.Index - 1]) && char.IsLowSurrogate(testString[m.Index])))
                        .ToArray()
                    : actualMatchEnumerable.ToArray();

                Assert.Equal(expectedMatches.Length, actualMatches.Length);

                for (var i = 0; i < actualMatches.Length; i++)
                {
                    var actualMatch = actualMatches[i];
                    var expectedMatch = expectedMatches[i];

                    Assert.Equal(expectedMatch.Index, actualMatch.Index);
                    Assert.Equal(expectedMatch.Captures.Length, actualMatch.Groups.Count);

                    var ignoreGroupCaptures = hintArray.Contains("!ignore-group-captures");
                    var captureCount = !ignoreGroupCaptures ? expectedMatch.Captures.Length : 1;

                    for (var j = 0; j < captureCount; j++)
                    {
                        var actualGroup = actualMatch.Groups[j];
                        var expectedCapture = expectedMatch.Captures[j];
                        if (expectedCapture is not null)
                        {
                            Assert.True(actualGroup.Success);
                            Assert.Equal(expectedCapture, actualGroup.Value);
                        }
                        else if (!hintArray.Contains("!ignore-undefined-captures"))
                        {
                            Assert.False(actualGroup.Success);
                        }
                    }

                    if (!ignoreGroupCaptures && expectedMatch.Groups is not null)
                    {
                        foreach (var kvp in expectedMatch.Groups)
                        {
                            var actualGroup = actualMatch.Groups[kvp.Key];
                            if (!actualGroup.Success)
                            {
                                actualGroup = actualMatch.Groups[Scanner.RegExpParser.EncodeGroupName(kvp.Key)];
                            }

                            Assert.True(actualGroup.Success);
                            Assert.Equal(kvp.Value, actualGroup.Value);
                        }
                    }
                }
            }
            else
            {
                Assert.False(parseResult.Success);
                Assert.Null(parseResult.Regex);
                Assert.NotNull(parseResult.ConversionError);
            }
        }
        else
        {
            ParserException ex;

            if (!hintArray.Contains("!skip-validation"))
            {
                ex = Assert.Throws<ParserException>(() => regexValidator.Parse());

                if (!hintArray.Contains("!ignore-error-message"))
                {
                    Assert.Equal($"Invalid regular expression: /{pattern}/{flags}: {syntaxError}", ex.Error?.Description);
                }
            }

            ex = Assert.Throws<ParserException>(() => regexConverter.Parse());

            if (expectedAdaptedPattern != ")inconvertible(")
            {
                if (!hintArray.Contains("!ignore-error-message"))
                {
                    Assert.Equal($"Invalid regular expression: /{pattern}/{flags}: {syntaxError}", ex.Error?.Description);
                }
            }
            else
            {
                Assert.StartsWith("Cannot convert regular expression", ex.Error?.Description, StringComparison.Ordinal);
            }
        }
    }

    private sealed record RegExpMatch(string[] Captures, int Index, Dictionary<string, string>? Groups)
    {
        public static (RegExpMatch[]?, string?) MatchesFrom(Expression expression)
        {
            // This parser logic must align with the shape returned by generate-matches.js.

            if (expression is Literal { TokenType: TokenType.StringLiteral } literal)
            {
                return (null, (string) literal.Value!);
            }

            return (expression.As<ArrayExpression>().Elements
                .Select(el => MatchFrom(el!.As<ObjectExpression>()))
                .ToArray(), null);
        }

        public static RegExpMatch MatchFrom(ObjectExpression expression)
        {
            string[]? captures = null;
            int? index = null;
            Dictionary<string, string>? groups = null;

            foreach (var property in expression.Properties.Cast<Property>())
            {
                switch ((string) property.Key.As<Literal>().Value!)
                {
                    case "captures":
                        captures = property.Value.As<ArrayExpression>().Elements
                            .Select(el => (string) el!.As<Literal>().Value!)
                            .ToArray();
                        break;
                    case "index":
                        index = checked((int) (double) property.Value.As<Literal>().Value!);
                        break;
                    case "groups":
                        groups = property.Value.As<ObjectExpression>().Properties
                            .Cast<Property>()
                            .ToDictionary(p => (string) p.Key.As<Literal>().Value!, p => (string) p.Value.As<Literal>().Value!);
                        break;
                }
            }

            return new RegExpMatch(
                captures ?? throw new FormatException(),
                index ?? throw new FormatException(),
                groups);
        }
    }
}
