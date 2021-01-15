using System.Collections.Generic;
using Xunit;

namespace Esprima.Tests
{
    public class ScannerTests
    {
        [Fact]
        public void CanScanMultiLineComment()
        {
            var scanner = new Scanner("var foo=1; /* \"330413500\" */", new ParserOptions {Comment = true});
            
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

            Assert.Equal(new[] {"11-28"}, results);
        }
    }
}