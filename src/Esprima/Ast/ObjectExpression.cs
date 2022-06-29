using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ObjectExpression : Expression
    {
        private readonly NodeList<Expression> _properties;

        public ObjectExpression(in NodeList<Expression> properties) : base(Nodes.ObjectExpression)
        {
            _properties = properties;
        }

        public ref readonly NodeList<Expression> Properties { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _properties; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Properties);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitObjectExpression(this);

        public ObjectExpression UpdateWith(in NodeList<Expression> properties)
        {
            if (NodeList.AreSame(properties, Properties))
            {
                return this;
            }

            return new ObjectExpression(properties);
        }
    }
}
