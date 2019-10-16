using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrayPattern : Node, BindingPattern
    {
        private readonly NodeList<IArrayPatternElement> _elements;

        public ArrayPattern(in NodeList<IArrayPatternElement> elements) :
            base(Nodes.ArrayPattern)
        {
            _elements = elements;
        }

        public ref readonly NodeList<IArrayPatternElement> Elements => ref _elements;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(_elements);
    }
}
