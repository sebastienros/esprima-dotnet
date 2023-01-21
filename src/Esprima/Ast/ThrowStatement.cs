using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Argument) })]
public sealed class ThrowStatement : Statement
{
    public ThrowStatement(Expression argument) : base(Nodes.ThrowStatement)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitThrowStatement(this);

    public ThrowStatement UpdateWith(Expression argument)
    {
        if (argument == Argument)
        {
            return this;
        }

        return new ThrowStatement(argument);
    }

}
