namespace Esprima.Ast
{
    public sealed class StaticMemberExpression : MemberExpression
    {
        public StaticMemberExpression(Expression obj, Expression property, bool optional)
            : base(obj, property, false, optional)
        {
        }
    }
}
