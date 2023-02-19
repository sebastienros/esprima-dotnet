using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(Expression) })]
public sealed partial class JsxExpressionContainer : JsxExpression
{
    public JsxExpressionContainer(Expression expression) : base(JsxNodeType.ExpressionContainer)
    {
        Expression = expression;
    }

    public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private JsxExpressionContainer Rewrite(Expression expression)
    {
        return new JsxExpressionContainer(expression);
    }
}
