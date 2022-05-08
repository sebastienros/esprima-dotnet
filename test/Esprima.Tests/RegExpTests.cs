using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Esprima.Test;
using Xunit;

namespace Esprima.Tests
{
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
            var options = new ParserOptions { AdaptRegexp = true };
            var token = new Scanner(code, options).ScanRegExp();
            return (Regex) token.Value;
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
            var parser = new JavaScriptParser(@"var O = " + regexp);
            var program = parser.ParseScript();

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
        [InlineData("a", "(?:[\\u0061])")]
        [InlineData("💩", "(?:(?:💩))")]
        public void ShouldConvertSetsToUnicode(string source, string expected)
        {
            Assert.Equal(expected, SerializeSet(source));

            string SerializeSet(string expression, bool inverted = false)
            {
                var sb = new StringBuilder();
                Scanner.AppendConvertUnicodeSet(sb, expression, inverted);
                return sb.ToString();
            }
        }

        [Fact]
        public void ShouldConvertSetsToUnicodeSpecial()
        {
            // These values are altered by XUnit if passed in InlineData to ShouldConvertSetsToUnicode
            Assert.Equal("(?:(?:\ud83d[\udca9-\udcab]))", SerializeSet("💩-💫"));
            Assert.Equal("(?:(?:[\ud800-\ud83c][\udc00-\udfff]|\ud83d[\udc00-\udca9])|[\\u0061-\\uFFFF])", SerializeSet("a-💩"));

            string SerializeSet(string expression, bool inverted = false)
            {
                var sb = new StringBuilder();
                Scanner.AppendConvertUnicodeSet(sb, expression, inverted);
                return sb.ToString();
            }
        }

        [Theory]
        [InlineData("a", "97:97")]
        [InlineData("💩", "128169:128169")]
        [InlineData("💩-💫", "128169:128171")]
        public void ShouldCreateRangesFromRegEx(string source, string expected)
        {
            Assert.Equal(expected, SerializeRanges(source));

            string SerializeRanges(string expression)
            {
                var ranges = Scanner.CreateRanges(expression);
                return string.Join(";", ranges.Select(r => $"{r.Start}:{r.End}")).ToString();
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
            var scanner = new Scanner("", new ParserOptions { AdaptRegexp = true });
            var regex = scanner.ParseRegex("\\$", "gm");
            Assert.NotNull(regex);
        }

        [Fact]
        public void ShouldParseBundleJs()
        {
            // It contains very diverse unicode regular expression

            var path = Path.Combine(Fixtures.GetFixturesPath(), "Fixtures", "3rdparty", "bundle.js");
            var source = File.ReadAllText(path);
            var parser = new JavaScriptParser(source, new ParserOptions { AdaptRegexp = true });
            parser.ParseScript();
        }
    }
}
