using Esprima.Utils;

namespace Esprima.Ast;

public sealed class EmptyStatement : Statement
{
    public EmptyStatement() : base(Nodes.EmptyStatement)
    {
    }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitEmptyStatement(this);
}
