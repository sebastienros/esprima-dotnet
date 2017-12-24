namespace Esprima.Ast
{
    public class ConditionalExpression : Node,
        Expression
    {
        public readonly Expression Test;
        public readonly Expression Consequent;
        public readonly Expression Alternate;

        public ConditionalExpression(Expression test, Expression consequent, Expression alternate)
        {
            Type = Nodes.ConditionalExpression;
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }
    }
}