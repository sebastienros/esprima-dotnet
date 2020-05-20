namespace Esprima.Ast
{
    public sealed class IfStatement : Statement
    {
        public readonly Expression Test;
        public readonly Statement Consequent;
        public readonly Statement? Alternate;

        public IfStatement(
            Expression test,
            Statement consequent,
            Statement? alternate)
            : base(Nodes.IfStatement)
        {
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Test, Consequent, Alternate);
    }
}