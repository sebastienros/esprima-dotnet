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

        protected internal abstract object? Accept(AstVisitor visitor);
    }
}
