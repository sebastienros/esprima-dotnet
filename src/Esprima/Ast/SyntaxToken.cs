using System.Runtime.CompilerServices;

namespace Esprima.Ast;

public class SyntaxToken : SyntaxElement
{
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
        get => (RegexValue?) _additionalDataContainer.InternalData;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set => _additionalDataContainer.InternalData = value;
    }

    public override string ToString() => Value;
}
