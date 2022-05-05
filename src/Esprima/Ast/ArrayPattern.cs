using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ArrayPattern : BindingPattern
    {
        internal readonly NodeList<Expression?> _elements;

        public ArrayPattern(in NodeList<Expression?> elements) : base(Nodes.ArrayPattern)
        {
            _elements = elements;
        }

        public ReadOnlySpan<Expression?> Elements { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _elements.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_elements);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitArrayPattern(this);
        }

        public ArrayPattern UpdateWith(in NodeList<Expression?> elements)
        {
            if (NodeList.AreSame(elements, _elements))
            {
                return this;
            }

            return new ArrayPattern(elements);
        }
    }
}
