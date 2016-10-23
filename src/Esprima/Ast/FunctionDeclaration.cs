using System.Collections.Generic;

namespace Esprima.Ast
{
    public class FunctionDeclaration : Statement, Declaration, IFunction
    {
        public Identifier Id { get; set; }
        public IEnumerable<INode> Params { get; set; }
        public BlockStatement Body { get; set; }
        public bool Generator { get; set; }
        public bool Expression { get; set; }

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