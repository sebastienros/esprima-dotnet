using System.Runtime.CompilerServices;

namespace Esprima.Ast
{
    public abstract class ClassProperty : ClassElement, IProperty
    {
        protected ClassProperty(Nodes type, PropertyKind kind, Expression key, bool computed) : base(type)
        {
            Kind = kind;
            Key = key;
            Computed = computed;
        }

        public PropertyKind Kind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        /// <remarks>
        /// <see cref="Identifier"/> | <see cref="Literal"/> (string or numeric) | '[' <see cref="Expression"/> ']' | <see cref="PrivateIdentifier"/>
        /// </remarks>
        public Expression Key { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Computed { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public Expression? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => GetValue(); }
        protected abstract Expression? GetValue();
    }
}
