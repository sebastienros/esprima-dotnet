using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class DoWhileStatement : Statement
    {
        public readonly Statement Body;
        public readonly Expression Test;

        public DoWhileStatement(Statement body, Expression test) : base(Nodes.DoWhileStatement)
        {
            Body = body;
            Test = test;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Body, Test);
    }
}