using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ForOfStatement : Statement
    {
        public readonly INode Left;
        public readonly Expression Right;
        public readonly Statement Body;

        public ForOfStatement(INode left, Expression right, Statement body) :
            base(Nodes.ForOfStatement)
        {
            Left = left;
            Right = right;
            Body = body;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Left, Right, Body);
    }
}