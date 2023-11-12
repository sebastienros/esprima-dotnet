using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed partial class AwaitExpression : Expression
{
    public AwaitExpression(Expression argument) : base(Nodes.AwaitExpression)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AwaitExpression Rewrite(Expression argument)
    {
        return new AwaitExpression(argument);
    }
}
