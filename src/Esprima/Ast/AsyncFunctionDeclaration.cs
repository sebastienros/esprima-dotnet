using System.Collections.Generic;

namespace Esprima.Ast
{
    public class AsyncFunctionDeclaration : Statement, IFunctionDeclaration
    {
        private readonly NodeList<INode> _parameters;

        public AsyncFunctionDeclaration(
            Identifier id,
            in NodeList<INode> parameters,
            BlockStatement body,
            HoistingScope hoistingScope) :
            base(Nodes.FunctionDeclaration)
        {
            Id = id;
            _parameters = parameters;
            Body = body;
            HoistingScope = hoistingScope;
        }

        public Identifier Id { get; }
        public INode Body { get; }
        public bool Generator => false;
        public bool Expression => false;
        public bool Strict => true;
        public bool Async => true;
        public HoistingScope HoistingScope { get; }

        public ref readonly NodeList<INode> Params => ref _parameters;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Id, _parameters, Body);
    }
}