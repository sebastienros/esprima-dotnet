using Esprima.Utils;

namespace Esprima.Ast;

public sealed class Super : Expression
{
    public Super() : base(Nodes.Super)
    {
    }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitSuper(this);
}
