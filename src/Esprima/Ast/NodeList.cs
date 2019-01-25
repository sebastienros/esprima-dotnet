using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Esprima.JitHelper;

namespace Esprima.Ast
{
    public struct NodeList<T> : IReadOnlyList<T> where T : INode
    {
        private T[] _items;
        private int _count;

        internal NodeList(ICollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

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

        internal NodeList<TResult> Select<TResult>(Func<T, TResult> selector) where TResult : INode
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var list = new NodeList<TResult>
            {
                _count = Count,
                _items = new TResult[Count]
            };

            for (var i = 0; i < Count; i++)
            {
                list._items[i] = selector(_items[i]);
            }

            return list;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => index >= 0 && index < Count
                 ? _items[index]
                 : Throw<T>(new IndexOutOfRangeException());
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
            private int _index;
            private T[] _items; // Usually null when count is zero
            private int _count;

            internal Enumerator(T[] items, int count) : this()
            {
                _index = -1;
                _items = items;
                _count = count;
            }

            // Since the items can be null when count is zero, a negative
            // count is used to designate the disposed state.

            private bool IsDisposed => _count < 0;

            public void Dispose()
            {
                _items = null;
                _count = -1;
            }

            public bool MoveNext()
            {
                ThrowIfDisposed();

                if (_index + 1 == _count)
                {
                    return false;
                }

                _index++;
                return true;
            }

            public void Reset()
            {
                ThrowIfDisposed();
                _index = -1;
            }

            public T Current
            {
                get
                {
                    ThrowIfDisposed();
                    return _index >= 0
                         ? _items[_index]
                         : Throw<T>(new InvalidOperationException());
                }
            }

            object IEnumerator.Current => Current;

            private void ThrowIfDisposed()
            {
                if (IsDisposed)
                {
                    Throw<T>(new ObjectDisposedException(GetType().Name));
                }
            }
        }
    }

    public static class NodeList
    {
        internal static NodeList<T> From<T>(ref ArrayList<T> arrayList)
            where T : INode
        {
            arrayList.Yield(out var items, out var count);
            arrayList = default;
            return new NodeList<T>(items, count);
        }

        public static NodeList<T> Create<T>(NodeList<T> source) where T : INode =>
            source;

        public static NodeList<T> Create<T>(IEnumerable<T> source) where T : INode
        {
            switch (source)
            {
                case null:
                {
                    throw new ArgumentNullException(nameof(source));
                }

                case NodeList<T> list:
                {
                    return Create(list);
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