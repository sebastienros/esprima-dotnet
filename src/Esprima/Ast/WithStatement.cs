using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class WithStatement : Statement
    {
        public readonly Expression Object;
        public readonly Statement Body;

        public WithStatement(Expression obj, Statement body) : base(Nodes.WithStatement)
        {
            Object = obj;
            Body = body;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Object, Body);
    }
}