namespace Esprima.Ast
{
    public class IfStatement : Statement
    {
        public readonly Expression Test;
        public readonly Statement Consequent;
        public readonly Statement Alternate;

        public IfStatement(Expression test, Statement consequent, Statement alternate)
        {
            Type = Nodes.IfStatement;
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }

    }
}