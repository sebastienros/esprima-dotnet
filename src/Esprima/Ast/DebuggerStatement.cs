using System.Collections.Generic;

namespace Esprima.Ast
{
    public class DebuggerStatement: Statement
    {
        public DebuggerStatement() :
            base(Nodes.DebuggerStatement) {}

        public override IEnumerable<INode> ChildNodes => ZeroChildNodes;
    }
}