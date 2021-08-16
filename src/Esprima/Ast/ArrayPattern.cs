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

        public ref readonly NodeList<Expression?> Elements => ref _elements;

#pragma warning disable 8631
        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_elements);
#pragma warning restore 8631

        public override void Accept(AstVisitor visitor) => visitor.VisitArrayPattern(this);
    }
}
