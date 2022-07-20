using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class AssignmentPattern : Expression
    {
        internal Expression _right;

        public AssignmentPattern(Expression left, Expression right) : base(Nodes.AssignmentPattern)
        {
            Left = left;
            _right = right;
        }

        /// <summary>
        /// <see cref="Identifier"/> | <see cref="BindingPattern"/>
        /// </summary>
        public Expression Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _right; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Left, Right);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitAssignmentPattern(this);

        public AssignmentPattern UpdateWith(Expression left, Expression right)
        {
            if (left == Left && right == Right)
            {
                return this;
            }

            return new AssignmentPattern(left, right);
        }
    }
}
