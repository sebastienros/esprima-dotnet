#nullable disable

using System.Collections;
using System.Collections.Generic;

using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Ast
{
    /// <summary>
    /// Collection that enumerates nodes and node lists.
    /// </summary>
    public readonly struct NodeCollection : IReadOnlyList<Node>
    {
        internal static readonly NodeCollection Empty = new NodeCollection(0);

        private readonly Node _first;
        private readonly Node _second;
        private readonly Node _third;
        private readonly Node _fourth;
        private readonly Node[] _list1;
        private readonly int _list1Count;
        private readonly Node[] _list2;
        private readonly int _list2Count;
        private readonly Node _fifth;
        private readonly int _count;

        private readonly int _startNodeCount;

        private NodeCollection(int count)
            : this(null, null, null, null, null)
        {
            _count = count;
        }

        internal NodeCollection(Node first)
            : this(first, null, null, null, null)
        {
            _count = _startNodeCount = 1;
        }

        internal NodeCollection(Node first, Node second)
            : this(first, second, null, null, null)
        {
            _count = _startNodeCount = 2;
        }

        internal NodeCollection(Node first, Node second, Node third)
            : this(first, second, third, null, null)
        {
            _count = _startNodeCount = 3;
        }

        internal NodeCollection(Node first, Node second, Node third, Node fourth)
            : this(first, second, third, fourth, null)
        {
            _count = _startNodeCount = 4;
        }

        internal NodeCollection(Node[] first, int firstCount, Node second)
            : this(null, null, null, null, second)
        {
            _list1 = first;
            _list1Count = firstCount;
            _count = firstCount + 1;
        }

        internal NodeCollection(Node first, Node[] second, int secondCount)
            : this(first, null, null, null, null)
        {
            _startNodeCount = 1;
            _list1 = second;
            _list1Count = secondCount;
            _count = 1 + secondCount;
        }

        internal NodeCollection(Node[] first, int firstCount, Node[] second, int secondCount)
            : this(null, null, null, null, null)
        {
            _list1 = first;
            _list1Count = firstCount;
            _list2 = second;
            _list2Count = secondCount;
            _count = firstCount + secondCount;
        }

        internal NodeCollection(Node first, Node[] second, int secondCount, Node third)
            : this(first, null, null, null, third)
        {
            _startNodeCount = 1;
            _list1 = second;
            _list1Count = secondCount;
            _count = 1 + secondCount + 1;
        }

        internal NodeCollection(Node[] first, int firstCount)
            : this(null, null, null, null, null)
        {
            _list1 = first;
            _list1Count = firstCount;
            _count = firstCount;
        }

        private NodeCollection(
            Node first,
            Node second,
            Node third,
            Node fourth,
            Node fifth)
        {
            _startNodeCount = 0;
            _first = first;
            _second = second;
            _third = third;
            _fourth = fourth;

            _list1 = null;
            _list1Count = 0;
            
            _list2 = null;
            _list2Count = 0;

            _fifth = fifth;

            _count = 0;
        }

        public int Count => _count;

        public Node this[int index]
        {
            get
            {
                if (index >= _count)
                {
                    ThrowIndexOutOfRangeException();
                }

                if (index < _startNodeCount)
                {
                    switch (index)
                    {
                        case 0:
                            return _first;
                        case 1:
                            return _second;
                        case 2:
                            return _third;
                        case 3:
                            return _fourth;
                    }
                }

                index -= _startNodeCount;
                if (index < _list1Count)
                {
                    return _list1[index];
                }
                
                index -= _list1Count;
                if (index < _list2Count)
                {
                    return _list2[index];
                }

                return _fifth;
            }
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
                if (_index < collection.Count)
                {
                    _current = collection[_index];
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
    }
}