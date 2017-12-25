using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrayExpression : Node, Expression
    {
        public List<ArrayExpressionElement> Elements { get; }

        public ArrayExpression(List<ArrayExpressionElement> elements)
        {
            Type = Nodes.ArrayExpression;
            Elements = elements;
        }
    }
}