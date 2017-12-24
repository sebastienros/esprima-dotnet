using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrayPattern : Node, BindingPattern
    {
        public readonly List<ArrayPatternElement> Elements;

        public ArrayPattern(List<ArrayPatternElement> elements)
        {
            Type = Nodes.ArrayPattern;
            Elements = elements;
        }
    }
}
