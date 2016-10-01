namespace Esprima.Ast
{
    public class ThisExpression : Node,
        Expression
    {
        public ThisExpression()
        {
            Type = Nodes.ThisExpression;
        }
    }
}