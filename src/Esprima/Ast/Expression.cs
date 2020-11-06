namespace Esprima.Ast
{
    /// <summary>
    /// A JavaScript expression. 
    /// </summary>
    public abstract class Expression : StatementListItem
    {
        protected Expression(Nodes type) : base(type)
        {
        }
    }
}