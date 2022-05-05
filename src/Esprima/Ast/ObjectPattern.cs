using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ObjectPattern : BindingPattern
    {
        internal readonly NodeList<Node> _properties;

        public ObjectPattern(in NodeList<Node> properties) : base(Nodes.ObjectPattern)
        {
            _properties = properties;
        }

        public ReadOnlySpan<Node> Properties { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _properties.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_properties);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitObjectPattern(this);
        }

        public ObjectPattern UpdateWith(in NodeList<Node> properties)
        {
            if (NodeList.AreSame(properties, _properties))
            {
                return this;
            }

            return new ObjectPattern(properties);
        }
    }
}
