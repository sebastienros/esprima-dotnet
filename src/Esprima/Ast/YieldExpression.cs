using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed partial class YieldExpression : Expression
{
    public YieldExpression(Expression? argument, bool @delegate) : base(Nodes.YieldExpression)
    {
        Argument = argument;
        Delegate = @delegate;
    }

    public Expression? Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Delegate { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private YieldExpression Rewrite(Expression? argument)
    {
        return new YieldExpression(argument, Delegate);
    }
}
