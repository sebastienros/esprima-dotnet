using System.Collections.Generic;

namespace Esprima.Ast
{
    public class EmptyStatement : Statement
    {
        public EmptyStatement() :
            base(Nodes.EmptyStatement) {}

        public override IEnumerable<INode> ChildNodes => ZeroChildNodes;
    }
}