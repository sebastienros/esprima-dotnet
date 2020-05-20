using System.Runtime.CompilerServices;

namespace Esprima.Ast
{
    /// <summary>
    ///     Helps to succinctly implement <see cref="Node.ChildNodes" /> for subclasses of <see cref="Node" />.
    /// </summary>
    internal static class GenericChildNodeYield
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeCollection Yield<T>(Node? first, in NodeList<T> second, Node? third) where T : Node
        {
            return new NodeCollection(first, second._items, second._count, third);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeCollection Yield<T>(in NodeList<T> first) where T : Node
        {
            return new NodeCollection(first._items, first._count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeCollection Yield<T1, T2>(in NodeList<T1> first, in NodeList<T2> second) where T1 : Node where T2 : Node
        {
            return new NodeCollection(first._items, first._count, second._items, second._count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeCollection Yield<T>(Node? first, in NodeList<T> second) where T : Node
        {
            return new NodeCollection(first, second._items, second._count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NodeCollection Yield<T>(in NodeList<T> first, Node? second) where T : Node
        {
            return new NodeCollection(first._items, first._count, second);
        }
    }
}