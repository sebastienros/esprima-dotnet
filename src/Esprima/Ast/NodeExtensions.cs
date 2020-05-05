using System;
using System.Collections.Generic;
using System.Diagnostics;
using NodeSysList = System.Collections.Generic.List<Esprima.Ast.Node>;

namespace Esprima.Ast
{
    public static class NodeExtensions
    {
        [DebuggerStepThrough]
        public static T As<T>(this object node) where T : class 
        {
            return (T) node;
        }

        public static IEnumerable<Node> DescendantNodesAndSelf(this Node node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return DescendantNodes(new NodeSysList { node });
        }

        public static IEnumerable<Node> DescendantNodes(this Node node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return DescendantNodes(new NodeSysList(node.ChildNodes));
        }

        private static IEnumerable<Node> DescendantNodes(NodeSysList nodes)
        {
            while (nodes.Count > 0)
            {
                var node = nodes[0];
                nodes.RemoveAt(0);
                yield return node;
                nodes.InsertRange(0, node.ChildNodes);
            }
        }

        public static IEnumerable<Node> AncestorNodesAndSelf(this Node node, Node rootNode)
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

        public static IEnumerable<Node> AncestorNodes(this Node node, Node rootNode)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (rootNode == null)
            {
                throw new ArgumentNullException(nameof(rootNode));
            }

            var parents = new Stack<Node>();
            Search(rootNode);
            return parents;

            bool Search(Node aNode)
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