namespace Esprima.Ast
{
    public class Property : ClassProperty, ObjectExpressionProperty
    {
        public readonly bool Method;
        public readonly bool Shorthand;

        public Property(PropertyKind kind, Expression key, bool computed, PropertyValue value, bool method, bool shorthand) :
            base(Nodes.Property)
        {
            Key = key;
            Computed = computed;
            Value = value;
            Kind = kind;
            Method = method;
            Shorthand = shorthand;
        }
    }
}