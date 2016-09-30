namespace Esprima.Ast
{
    public class ExportStatement : Node, Statement
    {
        public Expression Expression;
        public ExportStatement(Expression expression)
        {
            Type = Nodes.ExpressionStatement;
            Expression = expression;
        }
    }
}
