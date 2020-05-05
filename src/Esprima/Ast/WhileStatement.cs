using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class WhileStatement : Statement
    {
        public readonly Expression Test;
        public readonly Statement Body;

        public WhileStatement(Expression test, Statement body) : base(Nodes.WhileStatement)
        {
            Test = test;
            Body = body;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Test, Body);
    }
}