namespace Esprima.Ast
{
    public class StaticMemberExpression : MemberExpression, PropertyKey
    {
        public StaticMemberExpression(Expression obj, Expression property) : base(obj, property, false)
        {
        }

        public string GetKey()
        {
            return ((PropertyKey) Object).GetKey() + "." + ((PropertyKey) Property).GetKey();
        }
    }
}
