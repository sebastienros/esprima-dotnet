using System.Collections.Generic;

namespace Esprima.Ast
{
    public class CallExpression : Node,
        Expression
    {
        public readonly Expression Callee;
        public readonly List<ArgumentListElement> Arguments;

        public bool Cached;
        public bool CanBeCached = true;
        public object CachedArguments;

        public CallExpression(Expression callee, List<ArgumentListElement> args)
        {
            Type = Nodes.CallExpression;
            Callee = callee;
            Arguments = args;
        }
    }
}