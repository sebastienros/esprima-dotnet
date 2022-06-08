using Esprima.Utils;

namespace Esprima.Ast;

public sealed class Decorator : Node
{
    public readonly Expression Expression;

    public Decorator(Expression expression) : base(Nodes.Decorator)
    {
        Expression = expression;
    }

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

        return new Decorator(expression).SetAdditionalInfo(this);
    }
}
