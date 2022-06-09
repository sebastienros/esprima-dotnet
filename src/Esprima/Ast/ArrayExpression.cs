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

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield<Expression>(Elements!);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitArrayExpression(this);
        }

        public ArrayExpression UpdateWith(in NodeList<Expression?> elements)
        {
            if (NodeList.AreSame(elements, Elements))
            {
                return this;
            }

            return new ArrayExpression(elements).SetAdditionalInfo(this);
        }
    }
}
