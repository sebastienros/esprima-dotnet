using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxExpressionContainer : JsxExpression
{
    public JsxExpressionContainer(Expression expression) : base(JsxNodeType.ExpressionContainer)
    {
        Expression = expression;
    }

    public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Expression);

    protected internal override T Accept<T>(IJsxAstVisitor<T> visitor) => visitor.VisitJsxExpressionContainer(this);

    public JsxExpressionContainer UpdateWith(Expression expression)
    {
        if (expression == Expression)
        {
            return this;
        }

        return new JsxExpressionContainer(expression);
    }
}
