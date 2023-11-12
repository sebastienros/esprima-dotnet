using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Expression) })]
public sealed partial class ChainExpression : Expression
{
    public ChainExpression(Expression expression) : base(Nodes.ChainExpression)
    {
        Expression = expression;
    }

    /// <remarks>
    /// <see cref="CallExpression"/> | <see cref="ComputedMemberExpression"/>| <see cref="StaticMemberExpression"/>
    /// </remarks>
    public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ChainExpression Rewrite(Expression expression)
    {
        return new ChainExpression(expression);
    }
}
