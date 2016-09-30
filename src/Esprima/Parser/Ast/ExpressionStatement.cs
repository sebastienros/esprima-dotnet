namespace Esprima.Ast
{
    public class ExpressionStatement : Node,
        Statement
    {
        public Expression Expression;

        public ExpressionStatement(Expression expression)
        {
            Type = Nodes.ExpressionStatement;
            Expression = expression;
        }
    }
}