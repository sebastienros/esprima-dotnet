namespace Esprima.Ast
{
    public sealed class StaticMemberExpression : MemberExpression
    {
        public StaticMemberExpression(Expression obj, Expression property) : base(obj, property, false)
        {
        }
    }
}
