using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ContinueStatement : Statement
    {
        public readonly Identifier Label;

        public ContinueStatement(Identifier label) :
            base(Nodes.ContinueStatement)
        {
            Label = label;
        }

        public override IEnumerable<Node> ChildNodes =>
            ChildNodeYielder.Yield(Label);
    }
}