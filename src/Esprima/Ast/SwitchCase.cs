using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SwitchCase : Node
    {
        public readonly Expression Test;
        public readonly List<StatementListItem> Consequent;

        public SwitchCase(Expression test, List<StatementListItem> consequent)
        {
            Type = Nodes.SwitchCase;
            Test = test;
            Consequent = consequent;
        }
    }
}