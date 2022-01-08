using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class MethodDeclaration : Declaration, IFunction
    {
        private readonly NodeList<Expression> _parameters;

        public MethodDeclaration(
            Identifier? id,
            in NodeList<Expression> parameters,
            BlockStatement body,
            bool generator,
            bool strict,
            bool async)
            : base(Nodes.MethodDeclaration)
        {
            Id = id;
            _parameters = parameters;
            Body = body;
            Generator = generator;
            Expression = false;
            Strict = strict;
            Async = async;
        }

        public Identifier? Id { get; }

        public Node Body { get; }
        public bool Generator { get; }
        public bool Expression { get; }
        public bool Async { get; }
        public bool Strict { get; }

        public ref readonly NodeList<Expression> Params => ref _parameters;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Id, _parameters, Body);

        protected internal override void Accept(AstVisitor visitor)
        {
            throw new System.Exception("Not supported - adhoc");
        }
    }
}
