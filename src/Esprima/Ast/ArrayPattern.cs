using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ArrayPattern : BindingPattern
    {
        private readonly NodeList<Expression?> _elements;

        public ArrayPattern(in NodeList<Expression?> elements) : base(Nodes.ArrayPattern)
        {
            _elements = elements;
        }

        public ref readonly NodeList<Expression?> Elements { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _elements; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullable(Elements);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitArrayPattern(this);

        public ArrayPattern UpdateWith(in NodeList<Expression?> elements)
        {
            if (NodeList.AreSame(elements, Elements))
            {
                return this;
            }

            return new ArrayPattern(elements);
        }
    }
}
