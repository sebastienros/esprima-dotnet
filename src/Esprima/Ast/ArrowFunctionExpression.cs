using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ArrowFunctionExpression : Expression, IFunction
    {
        private readonly NodeList<Expression> _params;

        public ArrowFunctionExpression(
            in NodeList<Expression> parameters,
            StatementListItem body,
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
        /// <summary>
        /// { <see cref="Identifier"/> | <see cref="BindingPattern"/> | <see cref="AssignmentPattern"/> | <see cref="RestElement"/> }
        /// </summary>
        public ref readonly NodeList<Expression> Params { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _params; }
        /// <remarks>
        /// <see cref="BlockStatement"/> | <see cref="Ast.Expression"/>
        /// </remarks>
        public StatementListItem Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        bool IFunction.Generator => false;
        public bool Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Params, Body);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitArrowFunctionExpression(this);

        public ArrowFunctionExpression UpdateWith(in NodeList<Expression> parameters, StatementListItem body)
        {
            if (NodeList.AreSame(parameters, Params) && body == Body)
            {
                return this;
            }

            return new ArrowFunctionExpression(parameters, body, Expression, Strict, Async);
        }
    }
}
