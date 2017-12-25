namespace Esprima.Ast
{
    public class YieldExpression : Node, Expression
    {
        public Expression Argument { get; }
        public bool Delegate { get; }

        public YieldExpression(Expression argument, bool delgate)
        {
            Type = Nodes.YieldExpression;
            Argument = argument;
            Delegate = delgate;
        }
    }
}