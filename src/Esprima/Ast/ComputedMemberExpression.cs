namespace Esprima.Ast
{
    public sealed class ComputedMemberExpression : MemberExpression
    {
        public ComputedMemberExpression(Expression obj, Expression property) : base(obj, property, true)
        {
        }
    }
}
