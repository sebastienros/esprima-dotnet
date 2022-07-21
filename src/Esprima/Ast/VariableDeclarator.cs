using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class VariableDeclarator : Node
    {
        public VariableDeclarator(Node id, Expression? init) :
            base(Nodes.VariableDeclarator)
        {
            Id = id;
            Init = init;
        }

        /// <remarks>
        /// <see cref="Identifier"/> | <see cref="BindingPattern"/>
        /// </remarks>
        public Node Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression? Init { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt1(Id, Init);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitVariableDeclarator(this);

        public VariableDeclarator UpdateWith(Node id, Expression? init)
        {
            if (id == Id && init == Init)
            {
                return this;
            }

            return new VariableDeclarator(id, init);
        }
    }
}
