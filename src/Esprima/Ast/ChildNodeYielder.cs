using System.Collections.Generic;

namespace Esprima.Ast
{
    /// <summary>
    /// Helps to succinctly implement <see cref="Node.ChildNodes"/> for
    /// subclasses of <see cref="Node"/>.
    /// </summary>

    internal static class ChildNodeYielder
    {
        /// <summary>
        /// Yields one to several nodes, skipping those that are
        /// <c>null</c>.
        /// </summary>

        public static IEnumerable<INode> Yield(
            INode first,
            INode second = null,
            INode third = null,
            INode fourth = null)
        {
            if (first  != null) yield return first;
            if (second != null) yield return second;
            if (third  != null) yield return third;
            if (fourth != null) yield return fourth;
        }

        /// <summary>
        /// Yields nodes of a list followed by optionally another node,
        /// skipping those arguments that <c>null</c>
        /// </summary>

        public static IEnumerable<INode> Yield<T>(
            NodeList<T> first, INode second = null)
            where T : class, INode
        {
            foreach (var node in first)
                yield return node;

            if (second != null)
            {
                yield return second;
            }
        }

        /// <summary>
        /// Yields nodes of lists, in the given order, skipping those
        /// lists that are <c>null</c>.
        /// </summary>

        public static IEnumerable<INode> Yield<T1, T2>(
            NodeList<T1> first, NodeList<T2> second)
            where T1 : class, INode
            where T2 : class, INode
        {
            foreach (var node in first)
                yield return node;

            foreach (var node in second)
                yield return node;
        }

        /// <summary>
        /// Yields a node followed by nodes of a list and finally, and
        /// optionally, another node. If any of the arguments are
        /// <c>null</c> then they are skipped.
        /// </summary>

        public static IEnumerable<INode> Yield<T>(
            INode first, NodeList<T> second, INode third = null)
            where T : class, INode
        {
            if (first != null)
            {
                yield return first;
            }

            foreach (var node in second)
                yield return node;

            if (third != null)
            {
                yield return third;
            }
        }
    }
}