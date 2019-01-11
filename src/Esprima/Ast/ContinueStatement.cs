using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ContinueStatement : Statement
    {
        public readonly Identifier Label;

        public ContinueStatement(Identifier label)
        {
            Type = Nodes.ContinueStatement;
            Label = label;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Label);
    }
}