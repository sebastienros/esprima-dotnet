using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class Property : ClassProperty
    {
        internal Expression _value;

        public Property(
            PropertyKind kind,
            Expression key,
            bool computed,
            Expression value,
            bool method,
            bool shorthand)
            : base(Nodes.Property, kind, key, computed)
        {
            _value = value;
            Method = method;
            Shorthand = shorthand;
        }

        public new Expression Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value; }
        protected override Expression? GetValue() => _value;

        public bool Method { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Shorthand { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Key, Value);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitProperty(this);
        }

        public Property UpdateWith(Expression key, Expression value)
        {
            if (key == Key && value == Value)
            {
                return this;
            }

            return new Property(Kind, key, Computed, value, Method, Shorthand).SetAdditionalInfo(this);
        }
    }
}
