using System.Runtime.CompilerServices;

namespace Esprima.Ast;

public sealed class RegExpLiteral : Literal
{
    public RegExpLiteral(string pattern, string flags, RegExpParseResult parseResult, string raw) : this(new RegexValue(pattern, flags), parseResult, raw)
    {
    }

    public RegExpLiteral(RegexValue regex, RegExpParseResult parseResult, string raw) : base(TokenType.RegularExpression, parseResult.Regex, raw)
    {
        // value is null if a Regex object couldn't be created out of the pattern or options

        Regex = regex;
        ParseResult = parseResult;
    }

    public RegexValue Regex { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public RegExpParseResult ParseResult { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
