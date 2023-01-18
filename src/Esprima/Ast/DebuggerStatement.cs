using Esprima.Utils;

namespace Esprima.Ast;

public sealed class DebuggerStatement : Statement
{
    public DebuggerStatement() : base(Nodes.DebuggerStatement) { }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => null;

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitDebuggerStatement(this);
}
