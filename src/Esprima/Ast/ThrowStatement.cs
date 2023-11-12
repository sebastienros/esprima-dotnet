using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed partial class ThrowStatement : Statement
{
    public ThrowStatement(Expression argument) : base(Nodes.ThrowStatement)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ThrowStatement Rewrite(Expression argument)
    {
        return new ThrowStatement(argument);
    }
}
