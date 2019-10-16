using System.Collections.Generic;

namespace Esprima.Ast
{
    public class CallExpression : Node,
        Expression
    {
        private readonly NodeList<ArgumentListElement> _arguments;

        public readonly Expression Callee;

        public CallExpression(Expression callee, in NodeList<ArgumentListElement> args) :
            base(Nodes.CallExpression)
        {
            Callee = callee;
            _arguments = args;
        }

        public ref readonly NodeList<ArgumentListElement> Arguments => ref _arguments;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Callee, _arguments);
    }
}
