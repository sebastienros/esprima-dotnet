using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class SpreadElement : Expression
{
    public SpreadElement(Expression argument) : base(Nodes.SpreadElement)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitSpreadElement(this);

    public SpreadElement UpdateWith(Expression argument)
    {
        if (argument == Argument)
        {
            return this;
        }

        return new SpreadElement(argument);
    }
}
