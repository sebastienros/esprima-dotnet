using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed partial class SpreadElement : Expression
{
    public SpreadElement(Expression argument) : base(Nodes.SpreadElement)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SpreadElement Rewrite(Expression argument)
    {
        return new SpreadElement(argument);
    }
}
