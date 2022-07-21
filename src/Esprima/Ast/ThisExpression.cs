using Esprima.Utils;

namespace Esprima.Ast;

public sealed class ThisExpression : Expression
{
    public ThisExpression() : base(Nodes.ThisExpression)
    {
    }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitThisExpression(this);
}
