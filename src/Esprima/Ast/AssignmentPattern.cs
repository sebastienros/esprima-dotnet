namespace Esprima.Ast
{
    public class AssignmentPattern :
        Node,
        Expression,
        ArrayPatternElement,
        FunctionParameter,
        PropertyValue
    {
        public INode Left;
        public Expression Right;

        public AssignmentPattern(INode left, Expression right)
        {
            Type = Nodes.AssignmentPattern;
            Left = left;
            Right = right;
        }
    }
}
