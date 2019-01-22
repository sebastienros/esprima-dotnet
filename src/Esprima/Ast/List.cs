﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Esprima.JitHelper;

namespace Esprima.Ast
{
    // This structure is like List<> from the BCL except it is designed
    // to be modifiable by this library during the construction of the AST
    // but publicly read-only thereafter. The only allocation required on the
    // heap is the backing array storage for the element. An empty list,
    // however causes no heap allocation; that is, the array is allocated
    // on first addition.

    public struct List<T> : IReadOnlyList<T>
    {
        private T[] _items;
        private int _count;

        internal List(int initialCapacity)
        {
            _items = initialCapacity == 0 ? null : new T[initialCapacity];
            _count = 0;
        }

        public List(List<T> list) : this()
        {
            if (list.Count <= 0)
            {
                return;
            }

            _items = new T[list.Count];
            list._items.CopyTo(_items, 0);
            _count = list.Count;
        }

        internal List(ICollection<T> collection) :
            this((collection ?? throw new ArgumentNullException(nameof(collection))).Count)
        {
            collection.CopyTo(_items, 0);
            _count = collection.Count;
        }

        private int Capacity => _items?.Length ?? 0;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        internal List<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var list = new List<TResult>
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

        internal void AddRange<TSource>(List<TSource> list) where TSource : T
        {
            if (list.Count == 0)
            {
                return;
            }

            var oldCount = Count;
            var newCount = oldCount + list.Count;

            if (Capacity < newCount)
            {
                Array.Resize(ref _items, newCount);
            }

            Debug.Assert(_items != null);
            Array.Copy(list._items, 0, _items, oldCount, list.Count);
            _count = newCount;
        }

        internal void Add(T item)
        {
            var capacity = Capacity;

            if (Count == capacity)
            {
                Array.Resize(ref _items, Math.Max(capacity * 2, 4));
            }

            Debug.Assert(_items != null);
            _items[Count] = item;
            _count++;
        }

        internal void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }

            _items[index] = default;
            _count--;

            if (index == Count)
            {
                return;
            }

            Array.Copy(_items, index + 1, _items, index, Count - index);
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => index >= 0 && index < Count
                 ? _items[index]
                 : Throw<T>(new IndexOutOfRangeException());

            internal set
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }

                _items[index] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Push(T item) => Add(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T Pop()
        {
            var lastIndex = Count - 1;
            var last = this[lastIndex];
            RemoveAt(lastIndex);
            return last;
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

    public static class List
    {
        public static List<T> Create<T>(List<T> source) =>
            source;

        public static List<T> Create<T>(IEnumerable<T> source)
        {
            switch (source)
            {
                case null:
                {
                    throw new ArgumentNullException(nameof(source));
                }

                case List<T> list:
                {
                    return Create(list);
                }

                case ICollection<T> collection:
                {
                    return collection.Count > 0
                         ? new List<T>(collection)
                         : default;
                }

                case IReadOnlyList<T> sourceList:
                {
                    if (sourceList.Count == 0)
                    {
                        return default;
                    }

                    var list = new List<T>(sourceList.Count);
                    for (var i = 0; i < sourceList.Count; i++)
                    {
                        list.Add(sourceList[i]);
                    }

                    return list;
                }

                default:
                {
                    var count
                        = source is IReadOnlyCollection<T> collection
                        ? collection.Count
                        : (int?)null;

                    var list = count is int initialCapacity
                             ? new List<T>(initialCapacity)
                             : new List<T>();

                    if (count == null || count > 0)
                    {
                        foreach (var item in source)
                        {
                            list.Add(item);
                        }
                    }

                    return list;
                }
            }
        }
    }
}