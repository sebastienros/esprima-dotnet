namespace Esprima.Ast;

public sealed class LogicalExpression : BinaryExpression
{
    public LogicalExpression(string op, Expression left, Expression right) : this(ParseBinaryOperator(op), left, right)
    {
    }

    public LogicalExpression(BinaryOperator op, Expression left, Expression right) : base(Nodes.LogicalExpression, op, left, right)
    {
        if (!IsLogicalOperator(op))
        {
            throw new ArgumentOutOfRangeException(nameof(op), op, "Value must be a logical operator.");
        }
    }

    internal static bool IsLogicalOperator(BinaryOperator op)
    {
        return op is
            BinaryOperator.LogicalAnd or
            BinaryOperator.LogicalOr or
            BinaryOperator.NullishCoalescing;
    }

    protected override BinaryExpression Rewrite(Expression left, Expression right)
    {
        return new LogicalExpression(Operator, left, right);
    }
}
