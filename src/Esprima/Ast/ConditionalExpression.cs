using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Test), nameof(Consequent), nameof(Alternate) })]
public sealed partial class ConditionalExpression : Expression
{
    public ConditionalExpression(
        Expression test,
        Expression consequent,
        Expression alternate) : base(Nodes.ConditionalExpression)
    {
        Test = test;
        Consequent = consequent;
        Alternate = alternate;
    }

    public Expression Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Consequent { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Alternate { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ConditionalExpression Rewrite(Expression test, Expression consequent, Expression alternate)
    {
        return new ConditionalExpression(test, consequent, alternate);
    }
}
