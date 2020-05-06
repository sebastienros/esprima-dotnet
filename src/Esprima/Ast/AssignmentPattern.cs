namespace Esprima.Ast
{
    public sealed class AssignmentPattern : Expression
    {
        public readonly Node Left;
        public Node Right;

        public AssignmentPattern(Node left, Node right) : base(Nodes.AssignmentPattern)
        {
            Left = left;
            Right = right;
        }

        public override NodeCollection ChildNodes => ChildNodeYielder.Yield(Left, Right);
    }
}
