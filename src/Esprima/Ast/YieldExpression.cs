namespace Esprima.Ast
{
    public sealed class YieldExpression : Expression
    {
        public readonly Expression? Argument;
        public readonly bool Delegate;

        public YieldExpression(Expression? argument, bool delgate) : base(Nodes.YieldExpression)
        {
            Argument = argument;
            Delegate = delgate;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Argument);
    }
}