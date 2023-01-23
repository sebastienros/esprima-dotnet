namespace Esprima.Ast;

[VisitableNode]
public sealed partial class ThisExpression : Expression
{
    public ThisExpression() : base(Nodes.ThisExpression)
    {
    }
}
