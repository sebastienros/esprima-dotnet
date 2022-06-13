using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class VariableDeclarator : Node
    {
        public VariableDeclarator(Expression id, Expression? init) :
            base(Nodes.VariableDeclarator)
        {
            Id = id;
            Init = init;
        }

        /// <remarks>
        /// BindingIdentifier | <see cref="BindingPattern"/>
        /// </remarks>
        public Expression Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression? Init { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Id, Init);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitVariableDeclarator(this);
        }

        public VariableDeclarator UpdateWith(Expression id, Expression? init)
        {
            if (id == Id && init == Init)
            {
                return this;
            }

            return new VariableDeclarator(id, init);
        }
    }
}
