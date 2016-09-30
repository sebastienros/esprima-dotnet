using System.Collections.Generic;

namespace Esprima.Ast
{
    public class CallExpression : Node,
        Expression
    {
        public Expression Callee;
        public List<ArgumentListElement> Arguments;

        //public bool Cached;
        //public bool CanBeCached = true;
        //public JsValue[] CachedArguments;

        public CallExpression(Expression callee, List<ArgumentListElement> args)
        {
            Type = Nodes.CallExpression;
            Callee = callee;
            Arguments = args;
        }
    }
}