namespace Esprima.Ast
{
    public abstract class MemberExpression : Node,
        Expression,
        ArrayPatternElement
    {
        public Expression Object;
        public Expression Property;

        // true if an indexer is used and the property to be evaluated
        public bool Computed;

        protected MemberExpression(Expression obj, Expression property, bool computed)
        {
            Type = Nodes.MemberExpression;

            Object = obj;
            Property = property;
            Computed = computed;
        }
    }
}