using System.Collections.Generic;

namespace Esprima.Ast
{
    public class CallExpression : Expression
    {
        private readonly NodeList<Expression> _arguments;

        public readonly Expression Callee;

        public CallExpression(Expression callee, in NodeList<Expression> args) :
            base(Nodes.CallExpression)
        {
            Callee = callee;
            _arguments = args;
        }

        public ref readonly NodeList<Expression> Arguments => ref _arguments;

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(Callee, _arguments);
    }
}
