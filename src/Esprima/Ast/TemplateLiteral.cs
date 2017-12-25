using System.Collections.Generic;

namespace Esprima.Ast
{
    public class TemplateLiteral : Node,
        Expression
    {
        public List<TemplateElement> Quasis { get; }
        public List<Expression> Expressions { get; }

        public TemplateLiteral(List<TemplateElement> quasis, List<Expression> expressions)
        {
            Type = Nodes.TemplateLiteral;
            Quasis = quasis;
            Expressions = expressions;
        }
    }
}