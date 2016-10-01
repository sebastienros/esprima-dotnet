namespace Esprima.Ast
{
    public class ThrowStatement : Node,
        Statement
    {
        public Expression Argument;

        public ThrowStatement(Expression argument)
        {
            Type = Nodes.ThrowStatement;
            Argument = argument;
        }
    }
}