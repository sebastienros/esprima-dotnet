using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Ast;
using static Esprima.JitHelper;

namespace Esprima
{
    // This structure is like List<> from the BCL except the only allocation
    // required on the heap is the backing array storage for the elements.
    // An empty list, however causes no heap allocation; that is, the array is
    // allocated on first addition.

    internal struct ArrayList<T> : IReadOnlyList<T>
    {
        private T[] _items;
        private int _count;

        private int[] _sharedVersion;
        private int _localVersion;

        public ArrayList(int initialCapacity)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            }

            _items = initialCapacity > 0 ? new T[initialCapacity] : null;
            _count = 0;
            _localVersion = 0;
            _sharedVersion = initialCapacity > 0 ? new[] { _localVersion } : null;
        }

        private int Capacity => _items?.Length ?? 0;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                AssertUnchanged();
                return _count;
            }
        }

        internal void AddRange<TSource>(ArrayList<TSource> list) where TSource : T
        {
            AssertUnchanged();

            var listCount = list.Count;
            if (listCount == 0)
            {
                return;
            }

            var oldCount = _count;
            var newCount = oldCount + listCount;

            if (Capacity < newCount)
            {
                if (_sharedVersion == null)
                {
                    _sharedVersion = new[] { 1 };
                }

                Array.Resize(ref _items, newCount);
            }

            Debug.Assert(_items != null);
            Array.Copy(list._items, 0, _items, oldCount, listCount);
            _count = newCount;

            OnChanged();
        }

        internal void Add(T item)
        {
            AssertUnchanged();

            var capacity = Capacity;

            if (_count == capacity)
            {
                if (_sharedVersion == null)
                {
                    _sharedVersion = new[] { 1 };
                }

                Array.Resize(ref _items, Math.Max(capacity * 2, 4));
            }

            Debug.Assert(_items != null);
            _items[_count] = item;
            _count++;

            OnChanged();
        }

        private void AssertUnchanged()
        {
            if (_localVersion != (_sharedVersion?[0] ?? 0))
            {
                throw new InvalidOperationException();
            }
        }

        private void OnChanged()
        {
            ref var version = ref _sharedVersion[0];
            version++;
            _localVersion = version;
        }

        internal void RemoveAt(int index)
        {
            AssertUnchanged();

            if (index < 0 || index >= _count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }

            _items[index] = default;
            _count--;

            if (index < _count - 1)
            {
                Array.Copy(_items, index + 1, _items, index, Count - index);
            }

            OnChanged();
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= _count)
                {
                    return Throw<T>(new IndexOutOfRangeException());
                }

                AssertUnchanged();
                return _items[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index < 0 || index >= _count)
                {
                    Throw<T>(new IndexOutOfRangeException());
                }

                AssertUnchanged();
                _items[index] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Push(T item) => Add(item);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T Pop()
        {
            var lastIndex = _count - 1;
            var last = this[lastIndex];
            RemoveAt(lastIndex);
            return last;
        }

        public void Yield(out T[] items, out int count)
        {
            items = _items;
            count = _count;
            this = default;
        }

        internal ArrayList<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var list = new ArrayList<TResult>
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

        public Enumerator GetEnumerator()
        {
            AssertUnchanged();
            return new Enumerator(_items, _count);
        }

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

    internal static class ArrayListExtensions
    {
        public static void AddRange<T>(this ref ArrayList<T> destination, NodeList<T> source)
            where T: INode
        {
            foreach (var item in source)
                destination.Add(item);
        }
    }
}