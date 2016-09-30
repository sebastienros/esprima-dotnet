namespace Esprima.Ast
{
    public class YieldExpression : Node,
        Expression
    {
        public Expression Argument;
        public bool Delegate;
        public YieldExpression(Expression argument, bool delgate)
        {
            Type = Nodes.YieldExpression;
            Argument = argument;
            Delegate = delgate;
        }
    }
}
