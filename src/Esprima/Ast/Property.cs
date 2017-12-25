namespace Esprima.Ast
{
    public class Property : ClassProperty
    {
        public bool Method { get; }
        public bool Shorthand { get; }

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