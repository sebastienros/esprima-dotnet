using System.Runtime.CompilerServices;
using Esprima.Utils;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

/// <summary>
/// A Jsx expression.
/// </summary>
public abstract class JsxExpression : Expression
{
    protected JsxExpression(JsxNodeType type) : base(Nodes.Extension)
    {
        Type = type;
    }

    public new JsxNodeType Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected abstract object? Accept(IJsxAstVisitor visitor, object? context);

    protected internal sealed override object? Accept(AstVisitor visitor, object? context)
    {
        return visitor is IJsxAstVisitor jsxVisitor ? Accept(jsxVisitor, context) : AcceptAsExtension(visitor, context);
    }
}
