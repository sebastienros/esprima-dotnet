using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SwitchStatement : Statement
    {
        public readonly Expression Discriminant;
        public readonly NodeList<SwitchCase> Cases;

        public SwitchStatement(Expression discriminant, NodeList<SwitchCase> cases) :
            base(Nodes.SwitchStatement)
        {
            Discriminant = discriminant;
            Cases = cases;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Discriminant, Cases);
    }
}