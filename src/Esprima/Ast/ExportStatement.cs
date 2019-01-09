namespace Esprima.Ast
{
    public class ExportStatement : Statement
    {
        public readonly Expression Expression;

        public ExportStatement(Expression expression) :
            base(Nodes.ExpressionStatement)
        {
            Expression = expression;
        }
    }
}