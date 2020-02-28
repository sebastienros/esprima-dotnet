using System.Collections.Generic;

namespace Esprima.Ast
{
    public sealed class AsyncFunctionExpression : Node, IFunctionExpression, PropertyValue
    {
        private readonly NodeList<INode> _parameters;

        public AsyncFunctionExpression(
            Identifier id,
            in NodeList<INode> parameters,
            BlockStatement body,
            HoistingScope hoistingScope) :
            base(Nodes.FunctionExpression)
        {
            Id = id;
            _parameters = parameters;
            Body = body;
            HoistingScope = hoistingScope;
        }

        public Identifier Id { get; }
        public ref readonly NodeList<INode> Params => ref _parameters;
        public INode Body { get; }
        public bool Generator => false;
        public bool Expression => false;
        public bool Strict => true;
        public bool Async => true;
        public HoistingScope HoistingScope { get; }
        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Id, _parameters, Body);
    }
}