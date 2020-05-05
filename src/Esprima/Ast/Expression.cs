namespace Esprima.Ast
{
    // an expression represents an actual value
    // foo() is an expression, a switch/case is a statement
    public abstract class Expression : Declaration
    {
        protected Expression(Nodes type) : base(type)
        {
        }
    }
}