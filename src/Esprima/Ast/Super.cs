namespace Esprima.Ast;

[VisitableNode]
public sealed partial class Super : Expression
{
    public Super() : base(Nodes.Super)
    {
    }
}
