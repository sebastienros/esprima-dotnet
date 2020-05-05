using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class ForInStatement : Statement
    {
        public readonly Node Left;
        public readonly Expression Right;
        public readonly Statement Body;
        public readonly bool Each;

        public ForInStatement(
            Node left, 
            Expression right,
            Statement body) : base(Nodes.ForInStatement)
        {
            Left = left;
            Right = right;
            Body = body;
            Each = false;
        }

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Left, Right, Body);
    }
}