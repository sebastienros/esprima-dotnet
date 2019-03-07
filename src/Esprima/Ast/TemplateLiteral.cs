using System.Collections.Generic;

namespace Esprima.Ast
{
    public class TemplateLiteral : Node,
        Expression
    {
        public readonly NodeList<TemplateElement> Quasis;
        public readonly NodeList<Expression> Expressions;

        public TemplateLiteral(NodeList<TemplateElement> quasis, NodeList<Expression> expressions) :
            base(Nodes.TemplateLiteral)
        {
            Quasis = quasis;
            Expressions = expressions;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Quasis, Expressions);
    }
}