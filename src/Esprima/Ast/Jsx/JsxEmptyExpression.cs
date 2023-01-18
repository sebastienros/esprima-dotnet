using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxEmptyExpression : JsxExpression
{
    public JsxEmptyExpression() : base(JsxNodeType.EmptyExpression)
    {
    }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override T Accept<T>(IJsxAstVisitor<T> visitor) => visitor.VisitJsxEmptyExpression(this);
}
