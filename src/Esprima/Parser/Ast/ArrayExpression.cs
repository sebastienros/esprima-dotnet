using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrayExpression : Node, Expression
    {
        public IEnumerable<ArrayExpressionElement> Elements;

        public ArrayExpression(IEnumerable<ArrayExpressionElement> elements)
        {
            Type = Nodes.ArrayExpression;
            Elements = elements;
        }
    }
}