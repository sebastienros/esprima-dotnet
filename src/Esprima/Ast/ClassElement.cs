namespace Esprima.Ast
{
    public abstract class ClassProperty : Expression
    {
        public readonly PropertyKind Kind;

        public readonly Expression Key; // Identifier, Literal, '[' Expression ']'
        public readonly bool Computed;

        public Expression? Value => GetValue();
        protected abstract Expression? GetValue();

        protected ClassProperty(Nodes type, PropertyKind kind, Expression key, bool computed) : base(type)
        {
            Kind = kind;
            Key = key;
            Computed = computed;
        }

        public override NodeCollection ChildNodes => new(Key, Value);
    }
}
