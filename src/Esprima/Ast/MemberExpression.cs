using System.Collections.Generic;

namespace Esprima.Ast
{
    public abstract class MemberExpression : Node,
        Expression,
        ArrayPatternElement
    {
        public readonly Expression Object;
        public readonly Expression Property;

        // true if an indexer is used and the property to be evaluated
        public readonly bool Computed;

        protected MemberExpression(Expression obj, Expression property, bool computed) :
            base(Nodes.MemberExpression)
        {
            Object = obj;
            Property = property;
            Computed = computed;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Object, Property);
    }
}