﻿using System.Text.RegularExpressions;
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
    }
}
