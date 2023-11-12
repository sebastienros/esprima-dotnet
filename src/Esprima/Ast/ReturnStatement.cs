using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed partial class ReturnStatement : Statement
{
    public ReturnStatement(Expression? argument) : base(Nodes.ReturnStatement)
    {
        Argument = argument;
    }

    public Expression? Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReturnStatement Rewrite(Expression? argument)
    {
        return new ReturnStatement(argument);
    }
}
