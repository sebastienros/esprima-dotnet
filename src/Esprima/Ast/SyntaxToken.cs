using System.Runtime.CompilerServices;

namespace Esprima.Ast;

public sealed class SyntaxToken : SyntaxElement
{
    public SyntaxToken(TokenType type, string value, RegexValue? regexValue = null)
    {
        Type = type;

        Value = value;
        RegexValue = regexValue;
    }

    public TokenType Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public string Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public RegexValue? RegexValue { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public override string ToString() => Value;
}
