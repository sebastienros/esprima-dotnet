using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esprima.Ast
{
    public class FunctionExpression : Node,
        IFunction,
        Expression,
        PropertyValue
    {
        public Identifier Id { get; }
        public List<INode> Params { get; }
        public BlockStatement Body { get; }
        public bool Generator { get; }
        public bool Expression { get; }

        [JsonIgnore]
        public HoistingScope HoistingScope { get; }

        [JsonIgnore]
        public bool Strict { get; }

        public FunctionExpression(
            Identifier id,
            List<INode> parameters,
            BlockStatement body,
            bool generator,
            HoistingScope hoistingScope,
            bool strict
            )
        {
            Type = Nodes.FunctionExpression;
            Id = id;
            Params = parameters;
            Body = body;
            Generator = generator;
            Expression = false;
            HoistingScope = hoistingScope;
            Strict = strict;
        }
    }
}