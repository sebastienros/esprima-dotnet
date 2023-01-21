using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed class YieldExpression : Expression
{
    public YieldExpression(Expression? argument, bool @delegate) : base(Nodes.YieldExpression)
    {
        Argument = argument;
        Delegate = @delegate;
    }

    public Expression? Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Delegate { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullable(Argument);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitYieldExpression(this);

    public YieldExpression UpdateWith(Expression? argument)
    {
        if (argument == Argument)
        {
            return this;
        }

        return new YieldExpression(argument, Delegate);
    }
}
