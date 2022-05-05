using Esprima.Utils;

namespace Esprima.Ast
{
    public abstract class MemberExpression : Expression
    {
        public readonly Expression Object;
        public readonly Expression Property;

        // true if an indexer is used and the property to be evaluated
        public readonly bool Computed;
        public readonly bool Optional;

        protected MemberExpression(Expression obj, Expression property, bool computed, bool optional)
            : base(Nodes.MemberExpression)
        {
            Object = obj;
            Property = property;
            Computed = computed;
            Optional = optional;
        }

        public override NodeCollection ChildNodes => new(Object, Property);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitMemberExpression(this) as T;
        }
    }
}
