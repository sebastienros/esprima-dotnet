using Microsoft.Extensions.Primitives;

namespace Esprima.Ast;

public sealed class UpdateExpression : UnaryExpression
{
    public UpdateExpression(StringSegment op, Expression arg, bool prefix) : base(Nodes.UpdateExpression, op, arg, prefix)
    {
    }

    public UpdateExpression(UnaryOperator op, Expression arg, bool prefix) : base(Nodes.UpdateExpression, op, arg, prefix)
    {
    }

    protected override UnaryExpression Rewrite(Expression argument)
    {
        return new UpdateExpression(Operator, argument, Prefix);
    }
}
