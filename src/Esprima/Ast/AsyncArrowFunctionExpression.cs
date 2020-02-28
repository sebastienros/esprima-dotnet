using System.Collections.Generic;

namespace Esprima.Ast
{
    public class AsyncArrowFunctionExpression : Node, Expression, IFunction
    {
        private readonly NodeList<INode> _params;

        public AsyncArrowFunctionExpression(
            in NodeList<INode> parameters,
            INode body,
            bool expression,
            HoistingScope hoistingScope) :
            base(Nodes.ArrowFunctionExpression)
        {
            _params = parameters;
            Body = body;
            Expression = expression;
            HoistingScope = hoistingScope;
        }

        public Identifier Id => null;
        public INode Body { get; } // : BlockStatement | Expression;
        public bool Generator => false;
        public bool Expression { get; }
        public bool Strict => true;
        public bool Async => true;
        public HoistingScope HoistingScope { get; }

        public ref readonly NodeList<INode> Params => ref _params;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Params, Body);        
    }
}
