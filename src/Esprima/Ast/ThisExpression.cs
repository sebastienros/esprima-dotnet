using Esprima.Utils;

namespace Esprima.Ast;

public sealed class ThisExpression : Expression
{
    public ThisExpression() : base(Nodes.ThisExpression)
    {
    }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitThisExpression(this);
}
