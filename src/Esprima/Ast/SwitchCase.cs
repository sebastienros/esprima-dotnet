namespace Esprima.Ast
{
    public class SwitchCase : Node
    {
        public readonly Expression Test;
        public readonly List<StatementListItem> Consequent;

        public SwitchCase(Expression test, List<StatementListItem> consequent) :
            base(Nodes.SwitchCase)
        {
            Test = test;
            Consequent = consequent;
        }
    }
}