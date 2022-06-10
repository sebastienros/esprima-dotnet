using System.Runtime.CompilerServices;

namespace Esprima.Ast
{
    public abstract class ClassProperty : Expression
    {
        protected ClassProperty(Nodes type, PropertyKind kind, Expression key, bool computed) : base(type)
        {
            Kind = kind;
            Key = key;
            Computed = computed;
        }

        public PropertyKind Kind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression Key { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }  // Identifier, Literal, '[' Expression ']'
        public bool Computed { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public Expression? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetValue(); }
        protected abstract Expression? GetValue();

        public override NodeCollection ChildNodes => new(Key, GetValue());
    }
}
