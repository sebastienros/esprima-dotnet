using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class PropertyDefinition : ClassProperty
    {
        internal readonly NodeList<Decorator> _decorators;

        public PropertyDefinition(
            Expression key,
            bool computed,
            Expression? value,
            bool isStatic,
            in NodeList<Decorator> decorators)
            : base(Nodes.PropertyDefinition, PropertyKind.Property, key, computed)
        {
            Value = value;
            Static = isStatic;
            _decorators = decorators;
        }

        public new Expression? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        protected override Expression? GetValue() => Value;

        public bool Static { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ReadOnlySpan<Decorator> Decorators { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _decorators.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        private IEnumerable<Node?> CreateChildNodes()
        {
            yield return Key;
            yield return Value;

            foreach (var node in _decorators)
            {
                yield return node;
            }
        }

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitPropertyDefinition(this);
        }

        public PropertyDefinition UpdateWith(Expression key, Expression? value, in NodeList<Decorator> decorators)
        {
            if (key == Key && value == Value && NodeList.AreSame(decorators, _decorators))
            {
                return this;
            }

            return new PropertyDefinition(key, Computed, value, Static, decorators);
        }
    }
}
