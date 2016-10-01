namespace Esprima.Ast
{
    public class ConditionalExpression : Node,
        Expression
    {
        public Expression Test;
        public Expression Consequent;
        public Expression Alternate;

        public ConditionalExpression(Expression test, Expression consequent, Expression alternate)
        {
            Type = Nodes.ConditionalExpression;
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }
    }
}