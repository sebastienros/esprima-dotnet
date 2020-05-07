using System.Runtime.CompilerServices;

namespace Esprima.Ast
{
    /// <summary>
    /// Helps to succinctly implement <see cref="Node.ChildNodes"/> for subclasses of <see cref="Node"/>.
    /// </summary>
    internal static class GenericChildNodeYield
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeCollection Yield<T>(Node first, NodeList<T> second, Node third) where T : Node
        {
            return new NodeCollection(first, new NodeCollection.NodeArray(second._items, second._count), third);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeCollection Yield<T>(NodeList<T> first) where T : Node
        {
            return new NodeCollection(new NodeCollection.NodeArray(first._items, first._count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeCollection Yield<T1, T2>(NodeList<T1> first, NodeList<T2> second) where T1 : Node where T2 : Node
        {
            return new NodeCollection(new NodeCollection.NodeArray(first._items, first._count), new NodeCollection.NodeArray(second._items, second._count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeCollection Yield<T>(Node first, NodeList<T> second) where T : Node
        {
            return new NodeCollection(first, new NodeCollection.NodeArray(second._items, second._count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeCollection Yield<T>(NodeList<T> first, Node second) where T : Node
        {
            return new NodeCollection(new NodeCollection.NodeArray(first._items, first._count), second);
        }
    }
}