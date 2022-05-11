using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class PropertyDefinition : ClassProperty
    {
        public readonly bool Static;

        public PropertyDefinition(
            Expression key,
            bool computed,
            Expression value,
            bool isStatic)
            : base(Nodes.PropertyDefinition)
        {
            Static = isStatic;
            Key = key;
            Computed = computed;
            Value = value;
        }

        protected internal override Node Accept(AstVisitor visitor)
        {
            return visitor.VisitPropertyDefinition(this);
        }
    }
}
