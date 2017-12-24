namespace Esprima.Ast
{
    public class ExportStatement : Statement
    {
        public readonly Expression Expression;

        public ExportStatement(Expression expression)
        {
            Type = Nodes.ExpressionStatement;
            Expression = expression;
        }
    }
}