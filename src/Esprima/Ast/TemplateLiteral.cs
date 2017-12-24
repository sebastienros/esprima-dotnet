using System.Collections.Generic;

namespace Esprima.Ast
{
    public class TemplateLiteral : Node,
        Expression
    {
        public readonly List<TemplateElement> Quasis;
        public readonly List<Expression> Expressions;

        public TemplateLiteral(List<TemplateElement> quasis, List<Expression> expressions)
        {
            Type = Nodes.TemplateLiteral;
            Quasis = quasis;
            Expressions = expressions;
        }
    }
}