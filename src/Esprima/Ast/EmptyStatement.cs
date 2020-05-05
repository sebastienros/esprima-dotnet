using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class EmptyStatement : Statement
    {
        public EmptyStatement() : base(Nodes.EmptyStatement)
        {
        }

        public override IEnumerable<Node> ChildNodes => ZeroChildNodes;
    }
}