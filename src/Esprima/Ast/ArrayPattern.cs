using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrayPattern : Node, BindingPattern
    {
        public readonly NodeList<IArrayPatternElement> Elements;

        public ArrayPattern(NodeList<IArrayPatternElement> elements) :
            base(Nodes.ArrayPattern)
        {
            Elements = elements;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Elements);
    }
}
