using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esprima.Ast
{
    public class FunctionDeclaration : Node,
        Declaration,
        Statement
    {
        public Identifier Id;
        public IEnumerable<INode> Params;
        public BlockStatement Body;
        public bool Generator;
        public bool Expression;

        public FunctionDeclaration(
            Identifier id,
            IEnumerable<INode> parameters,
            BlockStatement body,
            bool generator
            )
        {
            Type = Nodes.FunctionDeclaration;
            Id = id;
            Params = parameters;
            Body = body;
            Generator = generator;
            Expression = false;
            //VariableDeclarations = LeaveVariableScope(),
            //FunctionDeclarations = LeaveFunctionScope()
        }
    }
}