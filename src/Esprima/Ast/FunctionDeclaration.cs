using System.Collections.Generic;

namespace Esprima.Ast
{
    public class FunctionDeclaration : Statement, IDeclaration, IFunction
    {
        private readonly NodeList<INode> _parameters;

        public FunctionDeclaration(
            Identifier id,
            in NodeList<INode> parameters,
            BlockStatement body,
            bool generator,
            HoistingScope hoistingScope,
            bool strict,
            bool async) :
            base(Nodes.FunctionDeclaration)
        {
            Id = id;
            _parameters = parameters;
            Body = body;
            Generator = generator;
            Expression = false;
            HoistingScope = hoistingScope;
            Strict = strict;
            Async = async;
        }

        public Identifier Id { get; }
        public INode Body { get; }
        public bool Generator { get; }
        public bool Expression { get; }
        public bool Async { get; }

        public HoistingScope HoistingScope { get; }
        public bool Strict { get; }
        public ref readonly NodeList<INode> Params => ref _parameters;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Id, _parameters, Body);
    }
}