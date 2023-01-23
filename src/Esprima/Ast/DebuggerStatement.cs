namespace Esprima.Ast;

[VisitableNode]
public sealed partial class DebuggerStatement : Statement
{
    public DebuggerStatement() : base(Nodes.DebuggerStatement) { }
}
