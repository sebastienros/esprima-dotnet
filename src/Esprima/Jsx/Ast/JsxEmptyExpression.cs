using Esprima.Ast;
using Esprima.Utils;

namespace Esprima.Jsx.Ast;

public sealed class JsxEmptyExpression : JsxExpression
{
    public JsxEmptyExpression() : base(JsxNodeType.EmptyExpression)
    {
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxEmptyExpression(this);
    }
}
