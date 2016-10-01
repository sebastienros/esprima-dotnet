using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrayPattern : Node, BindingPattern
    {
        public IEnumerable<ArrayPatternElement> Elements;
        public ArrayPattern(IEnumerable<ArrayPatternElement> elements)
        {
            Type = Nodes.ArrayPattern;
            Elements = elements;
        }
    }
}
