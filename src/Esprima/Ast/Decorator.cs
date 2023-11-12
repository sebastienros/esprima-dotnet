using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Expression) })]
public sealed partial class Decorator : Node
{
    public Decorator(Expression expression) : base(Nodes.Decorator)
    {
        Expression = expression;
    }

    public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Decorator Rewrite(Expression expression)
    {
        return new Decorator(expression);
    }
}
