using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class IfStatement : Statement
    {
        public readonly Expression Test;
        public readonly Statement Consequent;
        public readonly Statement Alternate;

        public IfStatement(
            Expression test,
            Statement consequent,
            Statement alternate)
            : base(Nodes.IfStatement)
        {
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Test, Consequent, Alternate);
    }
}