using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class Property : Node, IProperty
    {
        internal Node _value;

        public Property(
            PropertyKind kind,
            Expression key,
            bool computed,
            Node value,
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
        /// <see cref="Identifier"/> | <see cref="Literal"/> (string or numeric) | '[' <see cref="Expression"/> ']'
        /// </remarks>
        public Expression Key { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Computed { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        /// <remarks>
        /// When property of an object literal: <see cref="Expression"/> (incl. <see cref="SpreadElement"/> and <see cref="FunctionExpression"/> for getters/setters/methods) <br />
        /// When property of an object binding pattern: <see cref="Identifier"/> | <see cref="BindingPattern"/> | <see cref="AssignmentPattern"/> | <see cref="RestElement"/>
        /// </remarks>
        public Node Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value; }
        Node? IProperty.Value => Value;

        public bool Method { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Shorthand { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextProperty(Key, Value, Shorthand);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitProperty(this);

        public Property UpdateWith(Expression key, Node value)
        {
            if (key == Key && value == Value)
            {
                return this;
            }

            return new Property(Kind, key, Computed, value, Method, Shorthand);
        }
    }
}
