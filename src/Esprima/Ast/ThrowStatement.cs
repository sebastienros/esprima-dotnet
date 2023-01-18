using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class ThrowStatement : Statement
{
    public ThrowStatement(Expression argument) : base(Nodes.ThrowStatement)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitThrowStatement(this);

    public ThrowStatement UpdateWith(Expression argument)
    {
        if (argument == Argument)
        {
            return this;
        }

        return new ThrowStatement(argument);
    }

}
