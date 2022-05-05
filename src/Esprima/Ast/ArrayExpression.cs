using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ArrayExpression : Expression
    {
        internal readonly NodeList<Expression?> _elements;

        public ArrayExpression(in NodeList<Expression?> elements) : base(Nodes.ArrayExpression)
        {
            _elements = elements;
        }

        public ReadOnlySpan<Expression?> Elements { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _elements.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_elements);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitArrayExpression(this);
        }

        public ArrayExpression UpdateWith(in NodeList<Expression?> elements)
        {
            if (NodeList.AreSame(elements, _elements))
            {
                return this;
            }

            return new ArrayExpression(elements);
        }
    }
}
