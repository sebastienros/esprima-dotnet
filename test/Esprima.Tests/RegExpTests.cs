using System.Text.RegularExpressions;
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
        public void ShouldNotAcceptOctalEspacesWithUnicodeFlag()
        {
            Assert.Throws<ParserException>(() => CreateRegex(@"/\1/u"));
            Assert.Throws<ParserException>(() => CreateRegex(@"/\251/u"));
            Assert.Throws<ParserException>(() => CreateRegex(@"/\00/u"));
            Assert.NotNull(CreateRegex(@"/\0/u")); // NULL == \u0000
            Assert.NotNull(CreateRegex(@"/\1/"));
        }

        [Fact]
        public void ShouldCheckGroupBalance()
        {
            Assert.Throws<ParserException>(() => CreateRegex(@"/(/"));
            Assert.Throws<ParserException>(() => CreateRegex(@"/)/"));
            Assert.Throws<ParserException>(() => CreateRegex(@"/[/"));
            Assert.NotNull(CreateRegex(@"/]/"));
            Assert.NotNull(CreateRegex(@"/{/"));
            Assert.NotNull(CreateRegex(@"/}/"));

            Assert.NotNull(CreateRegex(@"/[(]/"));
            Assert.NotNull(CreateRegex(@"/[)]/"));
            Assert.NotNull(CreateRegex(@"/[{]/"));
            Assert.NotNull(CreateRegex(@"/[}]/"));
            Assert.NotNull(CreateRegex(@"/[[]/"));

            Assert.Throws<ParserException>(() => CreateRegex(@"/(/u"));
            Assert.Throws<ParserException>(() => CreateRegex(@"/)/u"));
            Assert.Throws<ParserException>(() => CreateRegex(@"/[/u"));
            Assert.Throws<ParserException>(() => CreateRegex(@"/]/u"));
            Assert.Throws<ParserException>(() => CreateRegex(@"/{/u"));
            Assert.Throws<ParserException>(() => CreateRegex(@"/}/u"));

            Assert.NotNull(CreateRegex(@"/([-.*+?^${}()|[\]\/\\])/"));
        }

        [Fact]
        public void ShouldPreventInfiniteLoopWhenAdaptingMultiLine()
        {
            var scanner = new Scanner("", new ParserOptions { AdaptRegexp = true });
            var regex = scanner.ParseRegex("\\$", "gm");
            Assert.NotNull(regex);
        }
    }
}
