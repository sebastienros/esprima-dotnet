namespace Esprima.Ast
{
    public class RestElement : Node,
        ArrayPatternElement, Expression
    {
        // Identifier in esprima but not forced and
        // for instance ...i[0] is a SpreadElement
        // which is reinterpreted to RestElement with a ComputerMemberExpression

        public readonly Expression Argument;

        public RestElement(Expression argument)
        {
            Type = Nodes.RestElement;
            Argument = argument;
        }
    }
}