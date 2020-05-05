namespace Esprima.Ast
{
    public sealed class Property : ClassProperty
    {
        public readonly bool Method;
        public readonly bool Shorthand;

        public Property(
            PropertyKind kind,
            Expression key,
            bool computed, 
            Expression value,
            bool method,
            bool shorthand)
            : base(Nodes.Property)
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