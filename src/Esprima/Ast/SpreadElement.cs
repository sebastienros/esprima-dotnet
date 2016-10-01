namespace Esprima.Ast
{
    public class SpreadElement : Node,
        ArgumentListElement,
        ArrayExpressionElement,
        Expression
    {
        public Expression Argument;
        public SpreadElement(Expression argument)
        {
            Type = Nodes.SpreadElement;
            Argument = argument;
        }
    }
}
