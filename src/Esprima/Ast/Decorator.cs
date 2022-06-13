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

    public override NodeCollection ChildNodes => new(Expression);

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitDecorator(this);
    }

    public Decorator UpdateWith(Expression expression)
    {
        if (expression == Expression)
        {
            return this;
        }

        return new Decorator(expression);
    }
}
