using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SwitchCase : Node
    {
        private readonly NodeList<IStatementListItem> _consequent;

        public readonly Expression Test;

        public SwitchCase(Expression test, in NodeList<IStatementListItem> consequent) :
            base(Nodes.SwitchCase)
        {
            Test = test;
            _consequent = consequent;
        }

        public ref readonly NodeList<IStatementListItem> Consequent => ref _consequent;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Test, _consequent);
    }
}