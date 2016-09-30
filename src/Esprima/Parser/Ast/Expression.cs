namespace Esprima.Ast
{
    public interface Expression :
        INode,
        PropertyValue,
        Declaration,
        ArgumentListElement,
        ArrayExpressionElement
    {
        // an expression represents an actual value
        // foo() is an expression, a switch/case is a statement
    }
}