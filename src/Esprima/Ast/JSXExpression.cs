namespace Esprima.Ast
{
    /// <summary>
    /// A JSX expression. 
    /// </summary>
    public abstract class JSXExpression : Expression
    {
        protected JSXExpression(Nodes type) : base(type)
        {
        }
    }
}
