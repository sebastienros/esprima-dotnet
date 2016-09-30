namespace Esprima.Ast
{
    public class Super : Node, Expression
    {
        public Super()
        {
            Type = Nodes.Super;
        }
    }
}
