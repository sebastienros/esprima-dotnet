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

        public ref readonly NodeList<Expression?> Elements => ref _elements;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield<Expression>(_elements!);

        protected internal override Node? Accept(AstVisitor visitor)
        {
            return visitor.VisitArrayExpression(this);
        }
    }
}
