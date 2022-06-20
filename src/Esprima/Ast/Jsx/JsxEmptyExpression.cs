using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxEmptyExpression : JsxExpression
{
    public JsxEmptyExpression() : base(JsxNodeType.EmptyExpression)
    {
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected override object? Accept(IJsxAstVisitor visitor, object? context)
    {
        return visitor.VisitJsxEmptyExpression(this, context);
    }
}
