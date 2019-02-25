using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SwitchCase : Node
    {
        public readonly Expression Test;
        public readonly NodeList<IStatementListItem> Consequent;

        public SwitchCase(Expression test, NodeList<IStatementListItem> consequent) :
            base(Nodes.SwitchCase)
        {
            Test = test;
            Consequent = consequent;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Test, Consequent);
    }
}