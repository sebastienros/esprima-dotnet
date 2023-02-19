namespace Esprima.Ast;

[VisitableNode]
public sealed partial class EmptyStatement : Statement
{
    public EmptyStatement() : base(Nodes.EmptyStatement)
    {
    }
}
