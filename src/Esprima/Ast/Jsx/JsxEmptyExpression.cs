using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor))]
public sealed partial class JsxEmptyExpression : JsxExpression
{
    public JsxEmptyExpression() : base(JsxNodeType.EmptyExpression)
    {
    }
}
