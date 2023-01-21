using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor))]
public sealed class JsxClosingFragment : JsxExpression
{
    public JsxClosingFragment() : base(JsxNodeType.ClosingFragment)
    {
    }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected override object? Accept(IJsxAstVisitor visitor) => visitor.VisitJsxClosingFragment(this);
}
