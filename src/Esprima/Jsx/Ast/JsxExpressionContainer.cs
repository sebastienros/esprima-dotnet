using Esprima.Ast;
using Esprima.Utils;

namespace Esprima.Jsx.Ast;

public sealed class JsxExpressionContainer : JsxExpression
{
    public readonly Expression Expression;

    public JsxExpressionContainer(Expression expression) : base(JsxNodeType.ExpressionContainer)
    {
        Expression = expression;
    }

    public override NodeCollection ChildNodes => new(Expression);

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxExpressionContainer(this);
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
