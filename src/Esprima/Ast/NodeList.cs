#nullable disable

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Ast
{
    public readonly struct NodeList<T> : IReadOnlyList<T> where T : Node
    {
        internal readonly T[] _items;
        internal readonly int _count;

        internal NodeList(ICollection<T> collection)
        {
            collection ??= ThrowArgumentNullException<ICollection<T>>(nameof(collection));

            var count = _count = collection.Count;
            if ((_items = count == 0 ? null : new T[count]) != null)
            {
                collection.CopyTo(_items, 0);
            }
        }

        internal NodeList(T[] items, int count)
        {
            _items = items;
            _count = count;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public NodeList<Node> AsNodes() =>
            new NodeList<Node>(_items /* conversion by co-variance! */, _count);

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // Following trick can reduce the range check by one
                if ((uint) index >= (uint) _count)
                {
                    ThrowIndexOutOfRangeException();
                }

                return _items[index];
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(_items, Count);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <remarks>
        /// This implementation does not detect changes to the list
        /// during iteration and therefore the behaviour is undefined
        /// under those conditions.
        /// </remarks>

        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[] _items; // Usually null when count is zero
            private readonly int _count;

            private int _index;
            private T _current;

            internal Enumerator(T[] items, int count) : this()
            {
                _index = 0;
                _items = items;
                _count = count;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_index < _count) 
                {                                                     
                    _current = _items[_index];                    
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }
 
            private bool MoveNextRare()
            {                
                _index = _count + 1;
                _current = default;
                return false;                
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            public T Current => _current;

            object IEnumerator.Current
            {
                get
                {
                    if(_index == 0 || _index == _count + 1)
                    {
                        ThrowInvalidOperationException<object>();
                    }
                    return Current;
                }
            }
        }
    }

    public static class NodeList
    {
        internal static NodeList<T> From<T>(ref ArrayList<T> arrayList) where T :  Node
        {
            arrayList.Yield(out var items, out var count);
            arrayList = default;
            return new NodeList<T>(items, count);
        }

        public static NodeList<T> Create<T>(IEnumerable<T> source) where T : Node
        {
            switch (source)
            {
                case null:
                {
                    return ThrowArgumentNullException<NodeList<T>>(nameof(source));
                }

                case NodeList<T> list:
                {
                    return list;
                }

                case ICollection<T> collection:
                {
                    return collection.Count > 0
                         ? new NodeList<T>(collection)
                         : default;
                }

                case IReadOnlyList<T> sourceList:
                {
                    if (sourceList.Count == 0)
                    {
                        return default;
                    }

                    var list = new ArrayList<T>(sourceList.Count);
                    for (var i = 0; i < sourceList.Count; i++)
                    {
                        list.Add(sourceList[i]);
                    }

                    return From(ref list);
                }

                default:
                {
                    var count
                        = source is IReadOnlyCollection<T> collection
                        ? collection.Count
                        : (int?)null;

                    var list = count is int initialCapacity
                             ? new ArrayList<T>(initialCapacity)
                             : new ArrayList<T>();

                    if (count == null || count > 0)
                    {
                        foreach (var item in source)
                        {
                            list.Add(item);
                        }
                    }

                    return From(ref list);
                }
            }
        }
    }
}
