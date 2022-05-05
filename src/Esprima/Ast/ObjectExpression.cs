using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ObjectExpression : Expression
    {
        internal readonly NodeList<Expression> _properties;

        public ObjectExpression(in NodeList<Expression> properties) : base(Nodes.ObjectExpression)
        {
            _properties = properties;
        }

        public ReadOnlySpan<Expression> Properties { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _properties.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_properties);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitObjectExpression(this);
        }

        public ObjectExpression UpdateWith(in NodeList<Expression> properties)
        {
            if (NodeList.AreSame(properties, _properties))
            {
                return this;
            }

            return new ObjectExpression(properties);
        }
    }
}
