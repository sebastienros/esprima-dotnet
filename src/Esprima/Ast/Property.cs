using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class Property : Expression, IProperty
    {
        internal Expression _value;

        public Property(
            PropertyKind kind,
            Expression key,
            bool computed,
            Expression value,
            bool method,
            bool shorthand)
            : base(Nodes.Property)
        {
            Kind = kind;
            Key = key;
            Computed = computed;
            _value = value;
            Method = method;
            Shorthand = shorthand;
        }

        public PropertyKind Kind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        /// <remarks>
        /// <see cref="Identifier"/> | <see cref="Literal"/> | '[' <see cref="Expression"/> ']'
        /// </remarks>
        public Expression Key { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Computed { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public Expression Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value; }
        Expression? IProperty.Value => Value;

        public bool Method { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Shorthand { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Key, Value);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitProperty(this, context);
        }

        public Property UpdateWith(Expression key, Expression value)
        {
            if (key == Key && value == Value)
            {
                return this;
            }

            return new Property(Kind, key, Computed, value, Method, Shorthand);
        }
    }
}
