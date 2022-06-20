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

        public ref readonly NodeList<Node> Properties { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _properties; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Properties);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitObjectPattern(this, context);
        }

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
