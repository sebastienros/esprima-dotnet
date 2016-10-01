namespace Esprima.Ast
{
    public class EmptyStatement : Node,
        Statement
    {
        public EmptyStatement()
        {
            Type = Nodes.EmptyStatement;
        }
    }
}