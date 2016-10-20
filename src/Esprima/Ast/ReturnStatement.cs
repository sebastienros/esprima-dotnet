namespace Esprima.Ast
{
    public class ReturnStatement : Statement
    {
        public Expression Argument;

        public ReturnStatement(Expression argument)
        {
            Type = Nodes.ReturnStatement;
            Argument = argument;
        }

    }
}