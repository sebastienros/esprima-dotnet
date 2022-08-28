using System.Runtime.CompilerServices;

namespace Esprima.Ast.Jsx;

public sealed class JsxSyntaxToken : SyntaxToken
{
    public JsxSyntaxToken(JsxTokenType type, string value) : base(TokenType.Extension, value)
    {
        Type = type;
    }

    public new JsxTokenType Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
}
