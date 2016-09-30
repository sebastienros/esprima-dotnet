using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SwitchCase : Node
    {
        public Expression Test;
        public IEnumerable<StatementListItem> Consequent;

        public SwitchCase(Expression test, IEnumerable<StatementListItem> consequent)
        {
            Type = Nodes.SwitchCase;
            Test = test;
            Consequent = consequent;
        }
    }
}