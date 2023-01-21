using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed class AwaitExpression : Expression
{
    public AwaitExpression(Expression argument) : base(Nodes.AwaitExpression)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitAwaitExpression(this);

    public AwaitExpression UpdateWith(Expression argument)
    {
        if (argument == Argument)
        {
            return this;
        }

        return new AwaitExpression(argument);
    }
}
