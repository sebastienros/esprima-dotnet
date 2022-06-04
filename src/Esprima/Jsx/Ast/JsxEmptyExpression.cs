using Esprima.Ast;
using Esprima.Jsx.Utils;

namespace Esprima.Jsx.Ast;

public sealed class JsxEmptyExpression : JsxExpression
{
    public JsxEmptyExpression() : base(JsxNodeType.EmptyExpression)
    {
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxEmptyExpression(this);
    }
}
