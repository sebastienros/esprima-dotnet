using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxEmptyExpression : JsxExpression
{
    public JsxEmptyExpression() : base(Nodes.JSXEmptyExpression)
    {
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxEmptyExpression(this);
    }
}
