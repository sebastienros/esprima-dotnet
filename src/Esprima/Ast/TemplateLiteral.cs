﻿using System.Collections.Generic;

namespace Esprima.Ast
{
    public class TemplateLiteral : Node,
        Expression
    {
        public readonly List<TemplateElement> Quasis;
        public readonly List<Expression> Expressions;

        public TemplateLiteral(List<TemplateElement> quasis, List<Expression> expressions) :
            base(Nodes.TemplateLiteral)
        {
            Quasis = quasis;
            Expressions = expressions;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Quasis, Expressions);
    }
}