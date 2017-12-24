namespace Esprima.Ast
{
    public class ExpressionStatement : Statement
    {
        public readonly Expression Expression;

        public ExpressionStatement(Expression expression)
        {
            Type = Nodes.ExpressionStatement;
            Expression = expression;
        }
    }
}