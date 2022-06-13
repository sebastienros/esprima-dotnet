using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class FunctionExpression : Expression, IFunction
    {
        private readonly NodeList<Expression> _params;

        public FunctionExpression(
            Identifier? id,
            in NodeList<Expression> parameters,
            BlockStatement body,
            bool generator,
            bool strict,
            bool async) :
            base(Nodes.FunctionExpression)
        {
            Id = id;
            _params = parameters;
            Body = body;
            Generator = generator;
            Strict = strict;
            Async = async;
        }

        public Identifier? Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ref readonly NodeList<Expression> Params { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _params; }

        public BlockStatement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        Node IFunction.Body => Body;

        public bool Generator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        bool IFunction.Expression => false;
        public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Id, Params, Body);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitFunctionExpression(this);
        }

        public FunctionExpression UpdateWith(Identifier? id, in NodeList<Expression> parameters, BlockStatement body)
        {
            if (id == Id && NodeList.AreSame(parameters, Params) && body == Body)
            {
                return this;
            }

            return new FunctionExpression(id, parameters, body, Generator, Strict, Async);
        }
    }
}
