using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Test), nameof(Consequent), nameof(Alternate) })]
public sealed partial class IfStatement : Statement
{
    public IfStatement(
        Expression test,
        Statement consequent,
        Statement? alternate)
        : base(Nodes.IfStatement)
    {
        Test = test;
        Consequent = consequent;
        Alternate = alternate;
    }

    public Expression Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Statement Consequent { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Statement? Alternate { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IfStatement Rewrite(Expression test, Statement consequent, Statement? alternate)
    {
        return new IfStatement(test, consequent, alternate);
    }
}
