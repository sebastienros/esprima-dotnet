namespace Esprima.Ast
{
    public sealed class ComputedMemberExpression : MemberExpression
    {
        public ComputedMemberExpression(Expression obj, Expression property, bool optional)
            : base(obj, property, true, optional)
        {
        }
    }
}
