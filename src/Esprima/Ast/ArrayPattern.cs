using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrayPattern : Node, BindingPattern
    {
        public List<ArrayPatternElement> Elements { get; }

        public ArrayPattern(List<ArrayPatternElement> elements)
        {
            Type = Nodes.ArrayPattern;
            Elements = elements;
        }
    }
}
