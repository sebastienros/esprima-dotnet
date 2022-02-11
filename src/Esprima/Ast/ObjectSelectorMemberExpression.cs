namespace Esprima.Ast
{
    public sealed class ObjectSelectorMemberExpression : MemberExpression
    {
        public ObjectSelectorMemberExpression(Expression obj, Expression property, bool optional)
            : base(obj, property, true, optional)
        {
        }
    }
}
