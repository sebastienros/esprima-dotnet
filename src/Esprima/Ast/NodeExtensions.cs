using System.Diagnostics;
using System.Runtime.CompilerServices;
using NodeSysList = System.Collections.Generic.List<Esprima.Ast.Node>;

namespace Esprima.Ast;

public static class NodeExtensions
{
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T As<T>(this Node node) where T : Node
    {
        return (T) node;
    }

    public static IEnumerable<Node> DescendantNodesAndSelf(this Node node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        return Impl(node);

        static IEnumerable<Node> Impl(Node node)
        {
            var nodes = new NodeSysList(1) { node };
            do
            {
                var lastIndex = nodes.Count - 1;

                node = nodes[lastIndex];
                nodes.RemoveAt(lastIndex);

                yield return node;

                foreach (var childNode in node.ChildNodes)
                {
                    nodes.Add(childNode);
                }

                nodes.Reverse(lastIndex, nodes.Count - lastIndex);
            }
            while (nodes.Count > 0);
        }
    }

    public static IEnumerable<Node> DescendantNodes(this Node node)
    {
        return DescendantNodesAndSelf(node).Skip(1);
    }

    public static IEnumerable<Node> AncestorNodesAndSelf(this Node node, Node rootNode)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (rootNode is null)
        {
            throw new ArgumentNullException(nameof(rootNode));
        }

        if (node == rootNode)
        {
            return new[] { node };
        }

        return Impl(node, rootNode);

        static IEnumerable<Node> Impl(Node node, Node rootNode)
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
    }

    public static IEnumerable<Node> AncestorNodes(this Node node, Node rootNode)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        if (rootNode is null)
        {
            throw new ArgumentNullException(nameof(rootNode));
        }

        if (node == rootNode)
        {
            return Enumerable.Empty<Node>();
        }

        var parents = new Stack<Node>();
        Search(rootNode, node, parents);
        return parents;

        static bool Search(Node aNode, Node targetNode, Stack<Node> parents)
        {
            parents.Push(aNode);
            foreach (var childNode in aNode.ChildNodes)
            {
                if (childNode == targetNode)
                {
                    return true;
                }

                if (Search(childNode, targetNode, parents))
                {
                    return true;
                }
            }

            parents.Pop();
            return false;
        }
    }
}
