using System.Runtime.CompilerServices;

namespace Esprima.Ast
{
    /// <summary>
    /// Helps to succinctly implement <see cref="Node.ChildNodes"/> for
    /// subclasses of <see cref="Node"/>.
    /// </summary>
    internal static class ChildNodeYielder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Node.NodeCollection Yield<T>(
            Node first,
            NodeList<T> second,
            Node third) where T : Node
        {
            return new Node.NodeCollection(first, new Node.NodeArray(second._items, second._count), third);
        }
        
        public static Node.NodeCollection Yield(Node first, Node second = null, Node third = null, Node fourth = null)
        {
            return new Node.NodeCollection(first, second, third, fourth);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Node.NodeCollection Yield<T>(NodeList<T> first) where T : Node
        {
            return new Node.NodeCollection(new Node.NodeArray(first._items, first._count));
        }

        public static Node.NodeCollection Yield<T1, T2>(NodeList<T1> first, NodeList<T2> second) where T1 : Node where T2 : Node
        {
            return new Node.NodeCollection(new Node.NodeArray(first._items, first._count), new Node.NodeArray(second._items, second._count));
        }

        public static Node.NodeCollection Yield<T>(Node first, NodeList<T> second) where T : Node
        {
            return new Node.NodeCollection(first, new Node.NodeArray(second._items, second._count));
        }

        public static Node.NodeCollection Yield<T>(NodeList<T> first, Node second) where T : Node
        {
            return new Node.NodeCollection(new Node.NodeArray(first._items, first._count), second);
        }
    }
}