using System.Collections.Generic;

namespace Esprima.Ast
{
    public class CallExpression : Node,
        Expression
    {
        public readonly Expression Callee;
        public readonly NodeList<ArgumentListElement> Arguments;

        public bool Cached;
        public bool CanBeCached = true;
        public object CachedArguments;

        public CallExpression(Expression callee, NodeList<ArgumentListElement> args) :
            base(Nodes.CallExpression)
        {
            Callee = callee;
            Arguments = args;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Callee, Arguments);
    }
}