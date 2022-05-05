using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ArrowFunctionExpression : Expression, IFunction
    {
        internal readonly NodeList<Expression> _params;

        public ArrowFunctionExpression(
            in NodeList<Expression> parameters,
            Node body,
            bool expression,
            bool strict,
            bool async)
            : base(Nodes.ArrowFunctionExpression)
        {
            _params = parameters;
            Body = body;
            Expression = expression;
            Strict = strict;
            Async = async;
        }

        Identifier? IFunction.Id => null;
        public ReadOnlySpan<Expression> Params { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _params.AsSpan(); }

        /// <remarks>
        /// <see cref="BlockStatement" /> | <see cref="Ast.Expression" />
        /// </remarks>
        public Node Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        bool IFunction.Generator => false;
        public bool Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_params, Body);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitArrowFunctionExpression(this);
        }

        public ArrowFunctionExpression UpdateWith(in NodeList<Expression> parameters, Node body)
        {
            if (NodeList.AreSame(parameters, _params) && body == Body)
            {
                return this;
            }

            return new ArrowFunctionExpression(parameters, body, Expression, Strict, Async);
        }
    }
}
