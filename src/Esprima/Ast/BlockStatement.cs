using System.Collections.Generic;

namespace Esprima.Ast
{
    public class BlockStatement : Statement
    {
        public readonly List<StatementListItem> Body;

        public BlockStatement(List<StatementListItem> body) :
            base(Nodes.BlockStatement)
        {
            Body = body;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Body);
    }
}