namespace Esprima.Ast
{
    public class ThrowStatement : Statement
    {
        public readonly Expression Argument;

        public ThrowStatement(Expression argument)
        {
            Type = Nodes.ThrowStatement;
            Argument = argument;
        }
    }
}