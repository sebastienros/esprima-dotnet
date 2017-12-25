using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SwitchCase : Node
    {
        public Expression Test { get; }
        public List<StatementListItem> Consequent { get; }

        public SwitchCase(Expression test, List<StatementListItem> consequent)
        {
            Type = Nodes.SwitchCase;
            Test = test;
            Consequent = consequent;
        }
    }
}