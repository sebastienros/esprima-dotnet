namespace Esprima.Ast
{
    public class AssignmentPattern :
        Node,
        Expression,
        ArrayPatternElement,
        FunctionParameter,
        PropertyValue
    {
        public readonly INode Left;
        public INode Right;

        public AssignmentPattern(INode left, INode right) :
            base(Nodes.AssignmentPattern)
        {
            Left = left;
            Right = right;
        }
    }
}
