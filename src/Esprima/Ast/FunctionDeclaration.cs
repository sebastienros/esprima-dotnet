using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class FunctionDeclaration : Declaration, IFunction
    {
        internal readonly NodeList<Expression> _params;

        public FunctionDeclaration(
            Identifier? id,
            in NodeList<Expression> parameters,
            BlockStatement body,
            bool generator,
            bool strict,
            bool async)
            : base(Nodes.FunctionDeclaration)
        {
            Id = id;
            _params = parameters;
            Body = body;
            Generator = generator;
            Strict = strict;
            Async = async;
        }

        public Identifier? Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ReadOnlySpan<Expression> Params { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _params.AsSpan(); }

        public BlockStatement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        Node IFunction.Body => Body;

        public bool Generator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        bool IFunction.Expression => false;
        public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Id, _params, Body);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitFunctionDeclaration(this);
        }

        public FunctionDeclaration UpdateWith(Identifier? id, in NodeList<Expression> parameters, BlockStatement body)
        {
            if (id == Id && NodeList.AreSame(parameters, _params) && body == Body)
            {
                return this;
            }

            return new FunctionDeclaration(id, parameters, body, Generator, Strict, Async);
        }
    }
}
