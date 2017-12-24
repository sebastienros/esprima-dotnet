namespace Esprima.Ast
{
    public class YieldExpression : Node, Expression
    {
        public readonly Expression Argument;
        public readonly bool Delegate;

        public YieldExpression(Expression argument, bool delgate)
        {
            Type = Nodes.YieldExpression;
            Argument = argument;
            Delegate = delgate;
        }
    }
}