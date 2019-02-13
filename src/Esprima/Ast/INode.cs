using System;
using System.Collections.Generic;
using System.Diagnostics;
using NodeSysList = System.Collections.Generic.List<Esprima.Ast.INode>;

namespace Esprima.Ast
{
    public interface
        INode
    {
        Nodes Type { get; }
        Range Range { get; set; }
        Location Location { get; set; }
        IEnumerable<INode> ChildNodes { get; }
    }

    public static class NodeExtensions
    {
        [DebuggerStepThrough]
        public static T As<T>(this object node) where T : class
        {
            return (T) node;
        }

        public static IEnumerable<INode> DescendantNodesAndSelf(this INode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return DescendantNodes(new NodeSysList { node });
        }

        public static IEnumerable<INode> DescendantNodes(this INode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return DescendantNodes(new NodeSysList(node.ChildNodes));
        }

        private static IEnumerable<INode> DescendantNodes(NodeSysList nodes)
        {
            while (nodes.Count > 0)
            {
                var node = nodes[0];
                nodes.RemoveAt(0);
                yield return node;
                nodes.InsertRange(0, node.ChildNodes);
            }
        }

        public static IEnumerable<INode> AncestorNodesAndSelf(this INode node, INode rootNode)
        {
            using (var ancestor = node.AncestorNodes(rootNode).GetEnumerator())
            {
                if (ancestor.MoveNext())
                {
                    yield return node;

                    do
                    {
                        yield return ancestor.Current;
                    }
                    while (ancestor.MoveNext());
                }
            }
        }

        public static IEnumerable<INode> AncestorNodes(this INode node, INode rootNode)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (rootNode == null)
            {
                throw new ArgumentNullException(nameof(rootNode));
            }

            var parents = new Stack<INode>();
            Search(rootNode);
            return parents;

            bool Search(INode aNode)
            {
                parents.Push(aNode);
                foreach (var childNode in aNode.ChildNodes)
                {
                    if (childNode == node)
                        return true;

                    if (Search(childNode))
                        return true;
                }

                parents.Pop();
                return false;
            }
        }
    }
}