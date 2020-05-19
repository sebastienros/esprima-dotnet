namespace Esprima.Ast
{
    public sealed class ArrayPattern : BindingPattern
    {
        private readonly NodeList<Expression> _elements;

        public ArrayPattern(in NodeList<Expression> elements) : base(Nodes.ArrayPattern)
        {
            _elements = elements;
        }

        public ref readonly NodeList<Expression> Elements => ref _elements;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_elements);
    }
}
