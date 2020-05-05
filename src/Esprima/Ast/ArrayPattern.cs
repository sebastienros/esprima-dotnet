using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class ArrayPattern : BindingPattern
    {
        private readonly NodeList<ArrayPatternElement> _elements;

        public ArrayPattern(in NodeList<ArrayPatternElement> elements) : base(Nodes.ArrayPattern)
        {
            _elements = elements;
        }

        public ref readonly NodeList<ArrayPatternElement> Elements => ref _elements;

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(_elements);
    }
}
