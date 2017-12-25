namespace Esprima.Ast
{
    public class ExportStatement : Statement
    {
        public Expression Expression { get; }

        public ExportStatement(Expression expression)
        {
            Type = Nodes.ExpressionStatement;
            Expression = expression;
        }
    }
}