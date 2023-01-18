using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class Decorator : Node
{
    public Decorator(Expression expression) : base(Nodes.Decorator)
    {
        Expression = expression;
    }

    public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Expression);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitDecorator(this);

    public Decorator UpdateWith(Expression expression)
    {
        if (expression == Expression)
        {
            return this;
        }

        return new Decorator(expression);
    }
}
