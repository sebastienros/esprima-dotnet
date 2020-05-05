using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class DebuggerStatement: Statement
    {
        public DebuggerStatement() : base(Nodes.DebuggerStatement) {}

        public override IEnumerable<Node> ChildNodes => ZeroChildNodes;
    }
}