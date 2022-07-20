using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class FunctionDeclaration : Declaration, IFunction
    {
        private readonly NodeList<Node> _params;

        public FunctionDeclaration(
            Identifier? id,
            in NodeList<Node> parameters,
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
        /// <summary>
        /// { <see cref="Identifier"/> | <see cref="BindingPattern"/> | <see cref="AssignmentPattern"/> | <see cref="RestElement"/> }
        /// </summary>
        public ref readonly NodeList<Node> Params { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _params; }

        public BlockStatement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        StatementListItem IFunction.Body => Body;

        public bool Generator { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        bool IFunction.Expression => false;
        public bool Strict { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public bool Async { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt0(Id, Params, Body);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitFunctionDeclaration(this);

        public FunctionDeclaration UpdateWith(Identifier? id, in NodeList<Node> parameters, BlockStatement body)
        {
            if (id == Id && NodeList.AreSame(parameters, Params) && body == Body)
            {
                return this;
            }

            return new FunctionDeclaration(id, parameters, body, Generator, Strict, Async);
        }
    }
}
