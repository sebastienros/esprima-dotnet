using System.Collections.Generic;

namespace Esprima.Ast
{
    public class DebuggerStatement: Statement
    {
        public DebuggerStatement()
        {
            Type = Nodes.DebuggerStatement;
        }

        public override IEnumerable<INode> ChildNodes => ZeroChildNodes;
   }
}