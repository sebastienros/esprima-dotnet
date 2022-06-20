using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ArrayExpression : Expression
    {
        private readonly NodeList<Expression?> _elements;

        public ArrayExpression(in NodeList<Expression?> elements) : base(Nodes.ArrayExpression)
        {
            _elements = elements;
        }

        public ref readonly NodeList<Expression?> Elements { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _elements; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Elements);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitArrayExpression(this, context);
        }

        public ArrayExpression UpdateWith(in NodeList<Expression?> elements)
        {
            if (NodeList.AreSame(elements, Elements))
            {
                return this;
            }

            return new ArrayExpression(elements);
        }
    }
}
