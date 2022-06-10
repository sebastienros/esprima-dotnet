using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public abstract class Node
    {
        internal Range _range;
        internal Location _location;

        protected Node(Nodes type)
        {
            Type = type;
        }

        public Nodes Type { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public Range Range
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _range;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init => _range = value;
        }

        public Location Location
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _location;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            init => _location = value;
        }

        public abstract NodeCollection ChildNodes { get; }

        protected internal abstract object? Accept(AstVisitor visitor);
    }
}
