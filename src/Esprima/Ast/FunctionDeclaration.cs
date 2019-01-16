using System.Collections.Generic;

namespace Esprima.Ast
{
    public class FunctionDeclaration : Statement, Declaration, IFunction
    {
        public Identifier Id { get; }
        public List<INode> Params { get; }
        public BlockStatement Body { get; }
        public bool Generator { get; }
        public bool Expression { get; }

        public HoistingScope HoistingScope { get; }
        public bool Strict { get; }

        public FunctionDeclaration(
            Identifier id,
            List<INode> parameters,
            BlockStatement body,
            bool generator,
            HoistingScope hoistingScope,
            bool strict) :
            base(Nodes.FunctionDeclaration)
        {
            Id = id;
            Params = parameters;
            Body = body;
            Generator = generator;
            Expression = false;
            HoistingScope = hoistingScope;
            Strict = strict;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Id, Params, Body);
    }
}