namespace Esprima.Ast
{
    public class ComputedMemberExpression : MemberExpression
    {
        public ComputedMemberExpression(Expression obj, Expression property) : base(obj, property, true)
        {
        }
    }
}
