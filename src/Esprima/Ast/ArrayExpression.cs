using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrayExpression : Node, Expression
    {
        private readonly NodeList<ArrayExpressionElement> _elements;

        public ArrayExpression(in NodeList<ArrayExpressionElement> elements) :
            base(Nodes.ArrayExpression)
        {
            _elements = elements;
        }

        public ref readonly NodeList<ArrayExpressionElement> Elements => ref _elements;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(_elements);
    }
}