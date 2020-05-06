using System.Collections;
using System.Collections.Generic;

namespace Esprima.Ast
{
    public abstract class Node
    {
        protected static readonly NodeCollection ZeroChildNodes = NodeCollection.Empty;

        protected Node(Nodes type) => Type = type;

        public Nodes Type { get; }
        public Range Range { get; set; }    

        public Location Location { get; set; }

        public abstract NodeCollection ChildNodes { get; }

        public readonly struct NodeCollection : IEnumerable<Node>
        {
            internal static readonly NodeCollection Empty = new NodeCollection(0);

            internal readonly Node _first;
            internal readonly Node _second;
            internal readonly Node _third;
            internal readonly Node _fourth;
            internal readonly NodeArray _list1;
            internal readonly NodeArray _list2;
            internal readonly Node _fifth;

            internal readonly byte _startNodeCount;
            internal readonly byte _endNodeCount;

            private NodeCollection(int count)
                : this(null, null, null, null, default, default, null)
            {
            }
            
            internal NodeCollection(Node first, Node second = null, Node third = null, Node fourth = null)
                : this(first, second, third, fourth, default, default, null)
            {
                _startNodeCount = 1;
                if (second != null)
                {
                    _startNodeCount++;
                }
                if (third != null)
                {
                    _startNodeCount++;
                }
                if (fourth != null)
                {
                    _startNodeCount++;
                }
                _endNodeCount = 0;
            }

            internal NodeCollection(NodeArray first, Node second)
                : this(null, null, null, null, first, default, second)
            {
                _startNodeCount = 0;
                _endNodeCount = 1;
            }

            internal NodeCollection(Node first, NodeArray second)
                : this(first, null, null, null, second, default, null)
            {
                _startNodeCount = 1;
                _endNodeCount = 0;
            }

            internal NodeCollection(NodeArray first, NodeArray second)
                : this(null, null, null, null, first, second, null)
            {
                _startNodeCount = 0;
                _endNodeCount = 0;
            }

            internal NodeCollection(Node first, NodeArray second, Node third)
                : this(first, null, null, null, second, default, third)
            {
                _startNodeCount = 1;
                _endNodeCount = 1;
            }

            internal NodeCollection(NodeArray first)
                : this(null, null, null, null, first, default, null)
            {
                _startNodeCount = 0;
                _endNodeCount = 0;
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
                _endNodeCount = 0;
            }

            public int Count => _startNodeCount + _list1.Count + _list2.Count + _endNodeCount;

            public NodeEnumerator GetEnumerator()
            {
                return new NodeEnumerator(this);
            }

            IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
            {
                return new NodeEnumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        
        public struct NodeEnumerator : IEnumerator<Node>
        {
            private readonly NodeCollection _collection;
            private int _index;
            private Node _current;

            public NodeEnumerator(in NodeCollection collection)
            {
                _collection = collection;
                _index = _collection.Count == 0 ? -1 : 0;
                _current = null;
            }

            public void Dispose()
            {
            }
 
            public bool MoveNext()
            {
                if (_index < 0)
                {
                    return false;
                }
                
                var collection = _collection;
                if (_index < collection._startNodeCount)
                {
                    _current = _index switch
                    {
                        0 => collection._first,
                        1 => collection._second,
                        2 => collection._third,
                        3 => collection._fourth,
                        _ => ExceptionHelper.ThrowInvalidOperationException<Node>()
                    };
                    _index++;
                    return true;
                }

                var index = _index - collection._startNodeCount;
                if (index < collection._list1.Count)
                {
                    _current = collection._list1.Nodes[index];
                    _index++;
                    return true;
                }

                index -= collection._list1.Count;
                if (index < collection._list2.Count)
                {
                    _current = collection._list2.Nodes[index];
                    _index++;
                    return true;
                }

                index -= collection._list2.Count;
                if (index < collection._endNodeCount)
                {
                    _current = index == 0 
                        ? collection._fifth 
                        : ExceptionHelper.ThrowInvalidOperationException<Node>();

                    _index++;
                    return true;
                }

                return false;
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
