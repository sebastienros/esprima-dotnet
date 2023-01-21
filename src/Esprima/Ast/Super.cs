using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode]
public sealed class Super : Expression
{
    public Super() : base(Nodes.Super)
    {
    }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitSuper(this);
}
