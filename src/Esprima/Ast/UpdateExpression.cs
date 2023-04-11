namespace Esprima.Ast;

public sealed class UpdateExpression : UnaryExpression
{
    public UpdateExpression(string op, Expression arg, bool prefix) : this(ParseUnaryOperator(op), arg, prefix)
    {
    }

    public UpdateExpression(UnaryOperator op, Expression arg, bool prefix) : base(Nodes.UpdateExpression, op, arg, prefix)
    {
        if (!IsUpdateOperator(op))
        {
            throw new ArgumentOutOfRangeException(nameof(op), op, "Value must be an update operator.");
        }
    }

    internal static bool IsUpdateOperator(UnaryOperator op)
    {
        return op is
            UnaryOperator.Increment or
            UnaryOperator.Decrement;
    }

    protected override UnaryExpression Rewrite(Expression argument)
    {
        return new UpdateExpression(Operator, argument, Prefix);
    }
}
