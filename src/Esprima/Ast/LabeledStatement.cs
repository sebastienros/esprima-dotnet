using System.Collections.Generic;

namespace Esprima.Ast
{
    public class LabeledStatement : Statement
    {
        public readonly Identifier Label;
        public readonly Statement Body;

        public LabeledStatement(Identifier label, Statement body) :
            base(Nodes.LabeledStatement)
        {
            Label = label;
            Body = body;
            body.LabelSet = label;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Label, Body);
    }
}