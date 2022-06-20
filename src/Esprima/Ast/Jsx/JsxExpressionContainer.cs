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

    public override NodeCollection ChildNodes => new(Expression);

    protected override object? Accept(IJsxAstVisitor visitor, object? context)
    {
        return visitor.VisitJsxExpressionContainer(this, context);
    }

    public JsxExpressionContainer UpdateWith(Expression expression)
    {
        if (expression == Expression)
        {
            return this;
        }

        return new JsxExpressionContainer(expression);
    }
}
