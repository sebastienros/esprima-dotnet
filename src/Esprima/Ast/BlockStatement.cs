using System.Collections.Generic;

namespace Esprima.Ast
{
    public class BlockStatement : Statement
    {
        public readonly NodeList<IStatementListItem> Body;

        public BlockStatement(NodeList<IStatementListItem> body) :
            base(Nodes.BlockStatement)
        {
            Body = body;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Body);
    }
}