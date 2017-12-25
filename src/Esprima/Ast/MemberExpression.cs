namespace Esprima.Ast
{
    public abstract class MemberExpression : Node,
        Expression,
        ArrayPatternElement
    {
        public Expression Object { get; }
        public Expression Property { get; }

        // true if an indexer is used and the property to be evaluated
        public bool Computed { get; }

        protected MemberExpression(Expression obj, Expression property, bool computed)
        {
            Type = Nodes.MemberExpression;

            Object = obj;
            Property = property;
            Computed = computed;
        }
    }
}