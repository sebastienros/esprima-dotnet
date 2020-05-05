using System.Collections.Generic;

namespace Esprima.Ast
{
    public class AssignmentPattern : ArrayPatternElement
    {
        public readonly Node Left;
        public Node Right;

        public AssignmentPattern(Node left, Node right) :
            base(Nodes.AssignmentPattern)
        {
            Left = left;
            Right = right;
        }

        public override IEnumerable<Node> ChildNodes =>
            ChildNodeYielder.Yield(Left, Right);
    }
}
