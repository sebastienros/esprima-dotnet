using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SwitchCase : Node
    {
        public readonly Expression Test;
        public readonly List<IStatementListItem> Consequent;

        public SwitchCase(Expression test, List<IStatementListItem> consequent) :
            base(Nodes.SwitchCase)
        {
            Test = test;
            Consequent = consequent;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Test, Consequent);
    }
}