using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esprima.Ast
{
    public class CallExpression : Node, Expression
    {
        public Expression Callee { get; }
        public List<ArgumentListElement> Arguments { get; }

        [JsonIgnore]
        public bool Cached { get; set; }

        [JsonIgnore]
        public bool CanBeCached { get; set;} = true;

        [JsonIgnore]
        public object CachedArguments { get; set; }

        public CallExpression(Expression callee, List<ArgumentListElement> args)
        {
            Type = Nodes.CallExpression;
            Callee = callee;
            Arguments = args;
        }
    }
}