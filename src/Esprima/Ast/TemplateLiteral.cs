﻿using System.Collections.Generic;

namespace Esprima.Ast
{
    public class TemplateLiteral : Node,
        Expression
    {
        private readonly NodeList<TemplateElement> _quasis;
        private readonly NodeList<Expression> _expressions;

        public TemplateLiteral(
            in NodeList<TemplateElement> quasis,
            in NodeList<Expression> expressions) :
            base(Nodes.TemplateLiteral)
        {
            _quasis = quasis;
            _expressions = expressions;
        }

        public ref readonly NodeList<TemplateElement> Quasis => ref _quasis;
        public ref readonly NodeList<Expression> Expressions => ref _expressions;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(_quasis,  _expressions);
    }
}