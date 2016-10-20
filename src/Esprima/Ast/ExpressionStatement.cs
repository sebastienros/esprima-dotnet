namespace Esprima.Ast
{
    public class ExpressionStatement : Statement
    {
        public Expression Expression;

        public ExpressionStatement(Expression expression)
        {
            Type = Nodes.ExpressionStatement;
            Expression = expression;
        }
    }
}