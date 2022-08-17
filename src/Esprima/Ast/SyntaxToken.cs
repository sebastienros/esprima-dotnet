using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;

namespace Esprima.Ast;

public sealed class SyntaxToken : SyntaxElement
{
    public SyntaxToken(TokenType type, StringSegment value, RegexValue? regexValue = null)
    {
        Type = type;

        Value = value;
        RegexValue = regexValue;
    }

    public TokenType Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public StringSegment Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public RegexValue? RegexValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (RegexValue?) _additionalDataContainer.InternalData;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set => _additionalDataContainer.InternalData = value;
    }

    public override string ToString() => Value.ToString();
}
