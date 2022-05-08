using Esprima.Utils;

namespace Esprima.Ast;

public sealed class JsxEmptyExpression : JsxExpression
{
    public JsxEmptyExpression() : base(Nodes.JSXEmptyExpression)
    {
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected internal override T? Accept<T>(AstVisitor visitor) where T : class
    {
        return visitor.VisitJsxEmptyExpression(this) as T;
    }
}
