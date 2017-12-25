namespace Esprima.Ast
{
    public class ConditionalExpression : Node,
        Expression
    {
        public Expression Test { get; }
        public Expression Consequent { get; }
        public Expression Alternate { get; }

        public ConditionalExpression(Expression test, Expression consequent, Expression alternate)
        {
            Type = Nodes.ConditionalExpression;
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }
    }
}