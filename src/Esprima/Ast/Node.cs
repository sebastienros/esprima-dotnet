using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public abstract class Node
    {
        protected Node(Nodes type)
        {
            Type = type;
        }

        public Nodes Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public Range Range;
        public Location Location;

        public abstract NodeCollection ChildNodes { get; }

        protected internal abstract object? Accept(AstVisitor visitor, object? context);

        /// <summary>
        /// Dispatches the visitation of the current node to <see cref="AstVisitor.VisitExtension(Node, object?)"/>.
        /// </summary>
        /// <remarks>
        /// When defining custom node types, inheritors can use this method to implement the abstract <see cref="Accept(AstVisitor, object?)"/> method.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected object? AcceptAsExtension(AstVisitor visitor, object? context)
        {
            return visitor.VisitExtension(this, context);
        }
    }
}
