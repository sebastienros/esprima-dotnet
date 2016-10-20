namespace Esprima.Ast
{
    public class EmptyStatement : Statement
    {
        public EmptyStatement()
        {
            Type = Nodes.EmptyStatement;
        }
    }
}