namespace Esprima.Ast
{
    public class StaticMemberExpression : MemberExpression
    {
        public StaticMemberExpression(Expression obj, Expression property) : base(obj, property, false)
        {
        }
    }
}
