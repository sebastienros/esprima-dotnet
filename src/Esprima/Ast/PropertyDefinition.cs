using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class PropertyDefinition : ClassProperty
    {
        private readonly NodeList<Decorator> _decorators;

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
        public ref readonly NodeList<Decorator> Decorators { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _decorators; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        private IEnumerable<Node?> CreateChildNodes()
        {
            yield return Key;
            yield return Value;

            foreach (var node in Decorators)
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
            if (key == Key && value == Value && NodeList.AreSame(decorators, Decorators))
            {
                return this;
            }

            return new PropertyDefinition(key, Computed, value, Static, decorators).SetAdditionalInfo(this);
        }
    }
}
