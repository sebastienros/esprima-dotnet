namespace Esprima.Ast
{
    public class SpreadElement : Node,
        ArgumentListElement,
        ArrayExpressionElement,
        Expression
    {
        public readonly Expression Argument;

        public SpreadElement(Expression argument) :
            base(Nodes.SpreadElement)
        {
            Argument = argument;
        }
    }
}