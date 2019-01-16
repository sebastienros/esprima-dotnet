namespace Esprima.Ast
{
    public class ThisExpression : Node,
        Expression
    {
        public ThisExpression() :
            base(Nodes.ThisExpression) {}
    }
}