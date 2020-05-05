using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ArrayExpression : Expression
    {
        private readonly NodeList<Expression> _elements;

        public ArrayExpression(in NodeList<Expression> elements) :
            base(Nodes.ArrayExpression)
        {
            _elements = elements;
        }

        public ref readonly NodeList<Expression> Elements => ref _elements;

        public override IEnumerable<Node> ChildNodes =>
            ChildNodeYielder.Yield(_elements);
    }
}