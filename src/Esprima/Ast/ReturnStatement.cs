namespace Esprima.Ast
{
    public class ReturnStatement : Statement
    {
        public readonly Expression Argument;

        public ReturnStatement(Expression argument) :
            base(Nodes.ReturnStatement)
        {
            Argument = argument;
        }

    }
}