namespace Esprima.Ast
{
    public class IfStatement : Statement
    {
        public Expression Test;
        public Statement Consequent;
        public Statement Alternate;

        public IfStatement(Expression test, Statement consequent, Statement alternate)
        {
            Type = Nodes.IfStatement;
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }

    }
}