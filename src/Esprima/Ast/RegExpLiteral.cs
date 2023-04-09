using System.Runtime.CompilerServices;

namespace Esprima.Ast;

public sealed class RegExpLiteral : Literal
{
    public RegExpLiteral(string pattern, string flags, object? value, string raw) : base(TokenType.RegularExpression, value, raw)
    {
        // value is null if a Regex object couldn't be created out of the pattern or options

        Regex = new RegexValue(pattern, flags);
    }

    public RegexValue Regex { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
