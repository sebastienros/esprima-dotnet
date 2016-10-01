using System.Collections.Generic;

namespace Esprima.Ast
{
    public class TemplateLiteral : Node,
        Expression
    {
        public IEnumerable<TemplateElement> Quasis;
        public IEnumerable<Expression> Expressions;

        public TemplateLiteral(IEnumerable<TemplateElement> quasis, IEnumerable<Expression> expressions)
        {

            Type = Nodes.TemplateLiteral;
            Quasis = quasis;
            Expressions = expressions;
        }
    }
}
