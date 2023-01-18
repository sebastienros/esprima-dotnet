using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxClosingFragment : JsxExpression
{
    public JsxClosingFragment() : base(JsxNodeType.ClosingFragment)
    {
    }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override T Accept<T>(IJsxAstVisitor<T> visitor) => visitor.VisitJsxClosingFragment(this);
}
