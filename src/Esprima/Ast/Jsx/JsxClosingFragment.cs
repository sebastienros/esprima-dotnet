using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor))]
public sealed partial class JsxClosingFragment : JsxExpression
{
    public JsxClosingFragment() : base(JsxNodeType.ClosingFragment)
    {
    }
}
