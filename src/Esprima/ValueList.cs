﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Esprima.JitHelper;

namespace Esprima
{
    internal struct ValueList<T> : IReadOnlyList<T>
    {
        private T[] _items;
        private int _count;

        private int Capacity => _items?.Length ?? 0;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        internal void AddRange<TSource>(ValueList<TSource> list) where TSource : T
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
}