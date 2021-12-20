namespace Esprima.Ast
{
    public sealed class AttributeMemberExpression : MemberExpression
    {
        public AttributeMemberExpression(Expression obj, Expression property, bool optional)
            : base(obj, property, false, optional)
        {
        }
    }
}
