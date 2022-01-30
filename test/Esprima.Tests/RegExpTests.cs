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
        public void ShouldMatchPoop()
        {
            var regex = CreateRegex(@"/foo(.)bar/u");
            var matches = regex.Matches("foo💩bar");
            Assert.Equal("💩", matches[0].Groups[1].Value);
        }

        [Fact(Skip = ".NET can't parse the range")]
        public void ShouldMatchSymbols()
        {
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

        [Theory]
        [InlineData(@"/^[-!#$%&'*+.^_`|~A-Za-z0-9]*$/u")]
        [InlineData(@"/[\u0300-\u036F\u0483-\u0489\u0591-\u05BD\u05BF\u05C1\u05C2\u05C4\u05C5\u05C7\u0610-\u061A\u064B-\u065F\u0670\u06D6-\u06DC\u06DF-\u06E4\u06E7\u06E8\u06EA-\u06ED\u0711\u0730-\u074A\u07A6-\u07B0\u07EB-\u07F3\u07FD\u0816-\u0819\u081B-\u0823\u0825-\u0827\u0829-\u082D\u0859-\u085B\u0898-\u089F\u08CA-\u08E1\u08E3-\u0903\u093A-\u093C\u093E-\u094F\u0951-\u0957\u0962\u0963\u0981-\u0983\u09BC\u09BE-\u09C4\u09C7\u09C8\u09CB-\u09CD\u09D7\u09E2\u09E3\u09FE\u0A01-\u0A03\u0A3C\u0A3E-\u0A42\u0A47\u0A48\u0A4B-\u0A4D\u0A51\u0A70\u0A71\u0A75\u0A81-\u0A83\u0ABC\u0ABE-\u0AC5\u0AC7-\u0AC9\u0ACB-\u0ACD\u0AE2\u0AE3\u0AFA-\u0AFF\u0B01-\u0B03\u0B3C\u0B3E-\u0B44\u0B47\u0B48\u0B4B-\u0B4D\u0B55-\u0B57\u0B62\u0B63\u0B82\u0BBE-\u0BC2\u0BC6-\u0BC8\u0BCA-\u0BCD\u0BD7\u0C00-\u0C04\u0C3C\u0C3E-\u0C44\u0C46-\u0C48\u0C4A-\u0C4D\u0C55\u0C56\u0C62\u0C63\u0C81-\u0C83\u0CBC\u0CBE-\u0CC4\u0CC6-\u0CC8\u0CCA-\u0CCD\u0CD5\u0CD6\u0CE2\u0CE3\u0D00-\u0D03\u0D3B\u0D3C\u0D3E-\u0D44\u0D46-\u0D48\u0D4A-\u0D4D\u0D57\u0D62\u0D63\u0D81-\u0D83\u0DCA\u0DCF-\u0DD4\u0DD6\u0DD8-\u0DDF\u0DF2\u0DF3\u0E31\u0E34-\u0E3A\u0E47-\u0E4E\u0EB1\u0EB4-\u0EBC\u0EC8-\u0ECD\u0F18\u0F19\u0F35\u0F37\u0F39\u0F3E\u0F3F\u0F71-\u0F84\u0F86\u0F87\u0F8D-\u0F97\u0F99-\u0FBC\u0FC6\u102B-\u103E\u1056-\u1059\u105E-\u1060\u1062-\u1064\u1067-\u106D\u1071-\u1074\u1082-\u108D\u108F\u109A-\u109D\u135D-\u135F\u1712-\u1715\u1732-\u1734\u1752\u1753\u1772\u1773\u17B4-\u17D3\u17DD\u180B-\u180D\u180F\u1885\u1886\u18A9\u1920-\u192B\u1930-\u193B\u1A17-\u1A1B\u1A55-\u1A5E\u1A60-\u1A7C\u1A7F\u1AB0-\u1ACE\u1B00-\u1B04\u1B34-\u1B44\u1B6B-\u1B73\u1B80-\u1B82\u1BA1-\u1BAD\u1BE6-\u1BF3\u1C24-\u1C37\u1CD0-\u1CD2\u1CD4-\u1CE8\u1CED\u1CF4\u1CF7-\u1CF9\u1DC0-\u1DFF\u20D0-\u20F0\u2CEF-\u2CF1\u2D7F\u2DE0-\u2DFF\u302A-\u302F\u3099\u309A\uA66F-\uA672\uA674-\uA67D\uA69E\uA69F\uA6F0\uA6F1\uA802\uA806\uA80B\uA823-\uA827\uA82C\uA880\uA881\uA8B4-\uA8C5\uA8E0-\uA8F1\uA8FF\uA926-\uA92D\uA947-\uA953\uA980-\uA983\uA9B3-\uA9C0\uA9E5\uAA29-\uAA36\uAA43\uAA4C\uAA4D\uAA7B-\uAA7D\uAAB0\uAAB2-\uAAB4\uAAB7\uAAB8\uAABE\uAABF\uAAC1\uAAEB-\uAAEF\uAAF5\uAAF6\uABE3-\uABEA\uABEC\uABED\uFB1E\uFE00-\uFE0F\uFE20-\uFE2F\u101FD\u102E0\u10376-\u1037A\u10A01-\u10A03\u10A05\u10A06\u10A0C-\u10A0F\u10A38-\u10A3A\u10A3F\u10AE5\u10AE6\u10D24-\u10D27\u10EAB\u10EAC\u10F46-\u10F50\u10F82-\u10F85\u11000-\u11002\u11038-\u11046\u11070\u11073\u11074\u1107F-\u11082\u110B0-\u110BA\u110C2\u11100-\u11102\u11127-\u11134\u11145\u11146\u11173\u11180-\u11182\u111B3-\u111C0\u111C9-\u111CC\u111CE\u111CF\u1122C-\u11237\u1123E\u112DF-\u112EA\u11300-\u11303\u1133B\u1133C\u1133E-\u11344\u11347\u11348\u1134B-\u1134D\u11357\u11362\u11363\u11366-\u1136C\u11370-\u11374\u11435-\u11446\u1145E\u114B0-\u114C3\u115AF-\u115B5\u115B8-\u115C0\u115DC\u115DD\u11630-\u11640\u116AB-\u116B7\u1171D-\u1172B\u1182C-\u1183A\u11930-\u11935\u11937\u11938\u1193B-\u1193E\u11940\u11942\u11943\u119D1-\u119D7\u119DA-\u119E0\u119E4\u11A01-\u11A0A\u11A33-\u11A39\u11A3B-\u11A3E\u11A47\u11A51-\u11A5B\u11A8A-\u11A99\u11C2F-\u11C36\u11C38-\u11C3F\u11C92-\u11CA7\u11CA9-\u11CB6\u11D31-\u11D36\u11D3A\u11D3C\u11D3D\u11D3F-\u11D45\u11D47\u11D8A-\u11D8E\u11D90\u11D91\u11D93-\u11D97\u11EF3-\u11EF6\u16AF0-\u16AF4\u16B30-\u16B36\u16F4F\u16F51-\u16F87\u16F8F-\u16F92\u16FE4\u16FF0\u16FF1\u1BC9D\u1BC9E\u1CF00-\u1CF2D\u1CF30-\u1CF46\u1D165-\u1D169\u1D16D-\u1D172\u1D17B-\u1D182\u1D185-\u1D18B\u1D1AA-\u1D1AD\u1D242-\u1D244\u1DA00-\u1DA36\u1DA3B-\u1DA6C\u1DA75\u1DA84\u1DA9B-\u1DA9F\u1DAA1-\u1DAAF\u1E000-\u1E006\u1E008-\u1E018\u1E01B-\u1E021\u1E023\u1E024\u1E026-\u1E02A\u1E130-\u1E136\u1E2AE\u1E2EC-\u1E2EF\u1E8D0-\u1E8D6\u1E944-\u1E94A\uE0100-\uE01EF]/u")]
        public void ShouldParseBundleJsRegexes(string pattern)
        {
            Assert.NotNull(CreateRegex(pattern));
        }
    }
}
