using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esprima.Ast
{
    public class CallExpression : Node,
        Expression
    {
        public readonly Expression Callee;
        public readonly List<ArgumentListElement> Arguments;

        [JsonIgnore]
        public bool Cached;
        public bool CanBeCached = true;
        public object CachedArguments;

        [JsonIgnore]
        public bool CanBeCached = true;

        [JsonIgnore]
        public object CachedArguments;

        public CallExpression(Expression callee, List<ArgumentListElement> args)
        {
            Type = Nodes.CallExpression;
            Callee = callee;
            Arguments = args;
        }
    }
}