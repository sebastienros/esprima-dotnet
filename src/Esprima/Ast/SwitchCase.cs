using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SwitchCase : Node
    {
        private readonly NodeList<Statement> _consequent;
        public readonly Expression Test;

        public SwitchCase(Expression test, in NodeList<Statement> consequent) :
            base(Nodes.SwitchCase)
        {
            Test = test;
            _consequent = consequent;
        }

        public ref readonly NodeList<Statement> Consequent => ref _consequent;

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Test, _consequent);
    }
}