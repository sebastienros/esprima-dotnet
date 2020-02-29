using System.Collections.Generic;

namespace Esprima.Ast
{
    public class FunctionDeclaration : Statement, IFunctionDeclaration
    {
        private readonly NodeList<INode> _parameters;

        public FunctionDeclaration(
            Identifier id,
            in NodeList<INode> parameters,
            BlockStatement body,
            bool generator,
            bool strict,
            bool async,
            HoistingScope hoistingScope) :
            base(Nodes.FunctionDeclaration)
        {
            Id = id;
            _parameters = parameters;
            Body = body;
            Generator = generator;
            Expression = false;
            Strict = strict;
            Async = async;
            HoistingScope = hoistingScope;
        }

        public Identifier Id { get; }

        public INode Body { get; }
        public bool Generator { get; }
        public bool Expression { get; }
        public bool Async { get; }
        public bool Strict { get; }

        public HoistingScope HoistingScope { get; }
        public ref readonly NodeList<INode> Params => ref _parameters;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Id, _parameters, Body);
    }
}