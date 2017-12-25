namespace Esprima.Ast
{
    public class ThrowStatement : Statement
    {
        public Expression Argument { get; }

        public ThrowStatement(Expression argument)
        {
            Type = Nodes.ThrowStatement;
            Argument = argument;
        }
    }
}