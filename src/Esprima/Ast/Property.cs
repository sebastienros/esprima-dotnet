namespace Esprima.Ast
{
    public class Property : ClassProperty
    {
        public readonly bool Method;
        public readonly bool Shorthand;

        public Property(PropertyKind kind, PropertyKey key, bool computed, PropertyValue value, bool method, bool shorthand)
        {
            Type = Nodes.Property;
            Key = key;
            Computed = computed;
            Value = value;
            Kind = kind;
            Method = method;
            Shorthand = shorthand;
        }
    }
}