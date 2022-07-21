namespace Esprima.Ast;

public sealed class LogicalExpression : BinaryExpression
{
    public LogicalExpression(string op, Expression left, Expression right) : base(Nodes.LogicalExpression, op, left, right)
    {
    }

    public LogicalExpression(BinaryOperator op, Expression left, Expression right) : base(Nodes.LogicalExpression, op, left, right)
    {
    }

    protected override BinaryExpression Rewrite(Expression left, Expression right)
    {
        return new LogicalExpression(Operator, left, right);
    }
}
