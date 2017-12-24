using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrayExpression : Node, Expression
    {
        public readonly List<ArrayExpressionElement> Elements;

        public ArrayExpression(List<ArrayExpressionElement> elements)
        {
            Type = Nodes.ArrayExpression;
            Elements = elements;
        }
    }
}