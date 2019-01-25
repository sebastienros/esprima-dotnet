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

    [DebuggerDisplay("Count = {Count}, Capacity = {Capacity}, Version = {_localVersion}")]
    internal struct ArrayList<T> : IReadOnlyList<T>
    {
        private T[] _items;
        private int _count;

        // Having a struct intended for modification can introduce some very
        // subtle and ugly bugs if not used carefully. For example, two copies
        // of the struct start with the same base array and modifying either
        // list may also modify the other. Consider the following:
        //
        //     var a = new ArrayList<int>();
        //     a.Add(1);
        //     a.Add(2);
        //     a.Add(3);
        //     var b = a;
        //     b.Add(4);
        //     b.RemoveAt(0);
        //
        // Both `a` and `b` will see the same changes. However, they'll appear
        // to change independently if the example is changed as follows:
        //
        //     var a = new ArrayList<int>();
        //     a.Add(1);
        //     a.Add(2);
        //     a.Add(3);
        //     var b = a;
        //     b.Add(4);
        //     b.Add(5);        // <-- only new change
        //     b.RemoveAt(0);
        //
        // When 5 is added to `b`, `b` re-allocates its array to make space
        // and consequently further changes are only visible in `b`. To help
        // avoid these subtle bugs, the debug version of this implementation
        // tracks changes. It maintains a local and a boxed version number.
        // The boxed version gets shared by all copies of the struct. If a
        // modification is made via any copy then the boxed version number is
        // updated. Any subsequent use (even if for reading only) of other
        // copies check that their local version numbers haven't diverged from
        // the shared one. In effect, if a copy is made and modified then the
        // original will throw if ever used. For the example above, this
        // means while it's safe to continue to use copy `b` after
        // modification, `a` will become useless:
        //
        //     var a = new ArrayList<int>();
        //     a.Add(1);
        //     a.Add(2);
        //     a.Add(3);
        //     var b = a;
        //     b.Add(4);
        //     b.Add(5);
        //     b.RemoveAt(0);
        //     Console.WriteLine(b.Count); // safe to continue to use
        //     Console.WriteLine(a.Count); // will throw

#if DEBUG
        private int[] _sharedVersion;
        private int _localVersion;
#endif

        public ArrayList(int initialCapacity)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            }

            _items = initialCapacity > 0 ? new T[initialCapacity] : null;
            _count = 0;

#if DEBUG
            _localVersion = 0;
            _sharedVersion = initialCapacity > 0 ? new[] { _localVersion } : null;
#endif
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
                Resize(newCount);
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
                Resize(Math.Max(capacity * 2, 4));
            }

            Debug.Assert(_items != null);
            _items[_count] = item;
            _count++;

            OnChanged();
        }

        internal void Resize(int size)
        {
            #if DEBUG
            if (_sharedVersion == null)
            {
                _sharedVersion = new[] { 1 };
            }
            #endif

            Array.Resize(ref _items, size);
        }

        [Conditional("DEBUG")]
        private void AssertUnchanged()
        {
#if DEBUG
            if (_localVersion != (_sharedVersion?[0] ?? 0))
            {
                throw new InvalidOperationException();
            }
#endif
        }

        [Conditional("DEBUG")]
        private void OnChanged()
        {
#if DEBUG
            ref var version = ref _sharedVersion[0];
            version++;
            _localVersion = version;
#endif
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
            where T: class, INode
        {
            foreach (var item in source)
                destination.Add(item);
        }
    }
}