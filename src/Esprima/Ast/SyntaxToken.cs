using System.Runtime.CompilerServices;

namespace Esprima.Ast;

public class SyntaxToken : SyntaxElement
{
    private const int RegexValuePropertyIndex = 1;

    public SyntaxToken(TokenType type, string value, RegexValue? regexValue = null)
    {
        Type = type;

        Value = value;
        RegexValue = regexValue;
    }

    public TokenType Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public string Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public RegexValue? RegexValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (RegexValue?) GetDynamicPropertyValue(RegexValuePropertyIndex);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set => SetDynamicPropertyValue(RegexValuePropertyIndex, value);
    }

    public override string ToString() => Value;
}
