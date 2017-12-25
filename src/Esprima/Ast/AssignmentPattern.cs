namespace Esprima.Ast
{
    public class AssignmentPattern :
        Node,
        Expression,
        ArrayPatternElement,
        FunctionParameter,
        PropertyValue
    {
        public INode Left { get; }
        public INode Right { get; internal set; }

        public AssignmentPattern(INode left, INode right)
        {
            Type = Nodes.AssignmentPattern;
            Left = left;
            Right = right;
        }
    }
}
