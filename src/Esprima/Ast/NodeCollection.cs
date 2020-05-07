using System.Collections;
using System.Collections.Generic;

namespace Esprima.Ast
{
    /// <summary>
    /// Collection that enumerates nodes and node lists.
    /// </summary>
    public readonly struct NodeCollection : IEnumerable<Node>
    {
        internal static readonly NodeCollection Empty = new NodeCollection(0);

        private readonly Node _first;
        private readonly Node _second;
        private readonly Node _third;
        private readonly Node _fourth;
        private readonly NodeArray _list1;
        private readonly NodeArray _list2;
        private readonly Node _fifth;

        private readonly byte _startNodeCount;
        public readonly int Count;

        private NodeCollection(int count)
            : this(null, null, null, null, default, default, null)
        {
            Count = count;
        }

        internal NodeCollection(Node first)
            : this(first, null, null, null, default, default, null)
        {
            Count = _startNodeCount = 1;
        }

        internal NodeCollection(Node first, Node second)
            : this(first, second, null, null, default, default, null)
        {
            Count = _startNodeCount = 2;
        }

        internal NodeCollection(Node first, Node second, Node third)
            : this(first, second, third, null, default, default, null)
        {
            Count = _startNodeCount = 3;
        }

        internal NodeCollection(Node first, Node second, Node third, Node fourth)
            : this(first, second, third, fourth, default, default, null)
        {
            Count = _startNodeCount = 4;
        }

        internal NodeCollection(NodeArray first, Node second)
            : this(null, null, null, null, first, default, second)
        {
            Count = first.Count + 1;
        }

        internal NodeCollection(Node first, NodeArray second)
            : this(first, null, null, null, second, default, null)
        {
            _startNodeCount = 1;
            Count = 1 + second.Count;
        }

        internal NodeCollection(NodeArray first, NodeArray second)
            : this(null, null, null, null, first, second, null)
        {
            Count = first.Count + second.Count;
        }

        internal NodeCollection(Node first, NodeArray second, Node third)
            : this(first, null, null, null, second, default, third)
        {
            _startNodeCount = 1;
            Count = 1 + second.Count + 1;
        }

        internal NodeCollection(NodeArray first)
            : this(null, null, null, null, first, default, null)
        {
            Count = first.Count;
        }

        private NodeCollection(
            Node first,
            Node second,
            Node third,
            Node fourth,
            NodeArray list1,
            NodeArray list2,
            Node fifth)
        {
            _first = first;
            _second = second;
            _third = third;
            _fourth = fourth;
            _list1 = list1;
            _list2 = list2;
            _fifth = fifth;

            _startNodeCount = 0;
            Count = 0;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<Node>
        {
            private readonly NodeCollection _collection;
            private int _index;
            private Node _current;

            public Enumerator(in NodeCollection collection)
            {
                _collection = collection;
                _index = 0;
                _current = null;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                var collection = _collection;
                var index = _index;
                if (index >= collection.Count)
                {
                    return false;
                }

                _index++;

                if (index < collection._startNodeCount)
                {
                    if (index == 0)
                    {
                        _current = collection._first;
                    }
                    else if (index == 1)
                    {
                        _current = collection._second;
                    }
                    else if (index == 2)
                    {
                        _current = collection._third;
                    }
                    else if (index == 3)
                    {
                        _current = collection._fourth;
                    }
                    return true;
                }

                index -= collection._startNodeCount;
                var list = collection._list1;
                if (list.Count == 0)
                {
                    list = collection._list2;
                }
                
                if (index < list.Count)
                {
                    _current = list.Nodes[index];
                    return true;
                }

                index -= collection._list1.Count;
                if (index < collection._list2.Count)
                {
                    _current = collection._list2.Nodes[index];
                    return true;
                }

                _current = collection._fifth;
                return true;
            }

            public Node Current => _current;

            object IEnumerator.Current => Current;

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default;
            }
        }

        internal readonly struct NodeArray
        {
            internal readonly Node[] Nodes;
            internal readonly int Count;

            public NodeArray(Node[] nodes, int count)
            {
                Nodes = nodes;
                Count = count;
            }
        }
    }
}