using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxClosingFragment : JsxExpression
{
    public JsxClosingFragment() : base(JsxNodeType.ClosingFragment)
    {
    }

    public override NodeCollection ChildNodes => NodeCollection.Empty;

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxClosingFragment(this);
    }
}
