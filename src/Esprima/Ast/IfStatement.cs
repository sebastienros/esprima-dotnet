namespace Esprima.Ast
{
    public class IfStatement : Statement
    {
        public Expression Test { get; }
        public Statement Consequent { get; }
        public Statement Alternate { get; }

        public IfStatement(Expression test, Statement consequent, Statement alternate)
        {
            Type = Nodes.IfStatement;
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }

    }
}