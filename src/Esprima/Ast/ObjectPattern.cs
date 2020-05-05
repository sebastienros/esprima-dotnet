﻿using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ObjectPattern : BindingPattern
    {
        private readonly NodeList<Node> _properties;

        public ObjectPattern(in NodeList<Node> properties) :
            base(Nodes.ObjectPattern)
        {
            _properties = properties;
        }

        public ref readonly NodeList<Node> Properties => ref _properties;

        public override IEnumerable<Node> ChildNodes =>
            ChildNodeYielder.Yield(_properties);
    }
}