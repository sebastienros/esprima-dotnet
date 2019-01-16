﻿using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrayPattern : Node, BindingPattern
    {
        public readonly List<ArrayPatternElement> Elements;

        public ArrayPattern(List<ArrayPatternElement> elements) :
            base(Nodes.ArrayPattern)
        {
            Elements = elements;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Elements);
    }
}
