using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ObjectPattern : BindingPattern
    {
        private readonly NodeList<Node> _properties;

        public ObjectPattern(in NodeList<Node> properties) : base(Nodes.ObjectPattern)
        {
            _properties = properties;
        }

        /// <summary>
        /// { <see cref="Property"/> | <see cref="RestElement"/> }
        /// </summary>
        public ref readonly NodeList<Node> Properties { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _properties; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Properties);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitObjectPattern(this);

        public ObjectPattern UpdateWith(in NodeList<Node> properties)
        {
            if (NodeList.AreSame(properties, Properties))
            {
                return this;
            }

            return new ObjectPattern(properties);
        }
    }
}
