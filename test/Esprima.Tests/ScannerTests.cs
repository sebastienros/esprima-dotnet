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
    public void ShouldRejectSurrogateRangeAsIdentifierStart()
    {
        var scanner = new Scanner(@"\u{d800}\u{dc00}");
        var ex = Assert.Throws<ParserException>(new Func<object>(() => scanner.Lex()));
        Assert.Equal(Messages.UnexpectedTokenIllegal, ex.Error?.Description);
    }

    [Fact]
    public void ShouldRejectSurrogateRangeAsIdentifierPart()
    {
        var scanner = new Scanner(@"a\u{d800}\u{dc00}");
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
}
