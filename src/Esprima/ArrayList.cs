using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Ast;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima;

// This structure is like List<> from the BCL except the only allocation
// required on the heap is the backing array storage for the elements.
// An empty list, however causes no heap allocation; that is, the array is
// allocated on first addition.

#if DEBUG
[DebuggerDisplay("Count = {Count}, Capacity = {Capacity}, Version = {_localVersion}")]
[DebuggerTypeProxy(typeof(ArrayList<>.DebugView))]
#endif
internal struct ArrayList<T> : IReadOnlyList<T>
{
    private const int MinAllocatedCount = 4;

    private T[]? _items;
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
    private int[]? _sharedVersion;
    private int _localVersion;
#endif

    public ArrayList(int initialCapacity)
    {
        if (initialCapacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCapacity), initialCapacity, null);
        }

        _items = initialCapacity > 0 ? new T[initialCapacity] : null;
        _count = 0;

#if DEBUG
        _localVersion = 0;
        _sharedVersion = null;
#endif
    }

    /// <remarks>
    /// Expects ownership of the array!
    /// </remarks>
    internal ArrayList(T[] items)
    {
        _items = items;
        _count = items.Length;

#if DEBUG
        _localVersion = 0;
        _sharedVersion = null;
#endif
    }

    public int Capacity
    {
        get
        {
            AssertUnchanged();
            return _items?.Length ?? 0;
        }
        set
        {
            AssertUnchanged();

            if (value < _count)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
            else if (value == (_items?.Length ?? 0))
            {
                return;
            }
            else if (value > 0)
            {
                T[] array = new T[value];
                if (_count > 0)
                {
                    Array.Copy(_items, 0, array, 0, _count);
                }
                _items = array;
            }
            else
            {
                _items = null;
            }

            OnChanged();
        }
    }

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            AssertUnchanged();
            return _count;
        }
    }

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            AssertUnchanged();

            // Following trick can reduce the range check by one
            if ((uint) index < (uint) _count)
            {
                return _items![index];
            }

            return ThrowArgumentOutOfRangeException<T>(nameof(index), index, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            AssertUnchanged();

            // Following trick can reduce the range check by one
            if ((uint) index < (uint) _count)
            {
                _items![index] = value;
                return;
            }

            ThrowArgumentOutOfRangeException<T>(nameof(index), index, null);
        }
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
        _sharedVersion ??= new[] { 0 };

        ref var version = ref _sharedVersion[0];
        version++;
        _localVersion = version;
#endif
    }

    public void AddRange<TSource>(ArrayList<TSource> list) where TSource : T
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
            Array.Resize(ref _items, Math.Max(newCount, MinAllocatedCount));
        }

        Debug.Assert(_items is not null);
        Array.Copy(list._items, 0, _items, oldCount, listCount);
        _count = newCount;

        OnChanged();
    }

    public void Add(T item)
    {
        AssertUnchanged();

        var capacity = Capacity;

        if (_count == capacity)
        {
            Array.Resize(ref _items, Math.Max(capacity * 2, MinAllocatedCount));
        }

        Debug.Assert(_items is not null);
        _items![_count] = item;
        _count++;

        OnChanged();
    }

    public void Clear()
    {
        AssertUnchanged();

        if (_count > 0)
        {
            Array.Clear(_items, 0, _count);
            _count = 0;
        }

        OnChanged();
    }

    public void Insert(int index, T item)
    {
        AssertUnchanged();

        if ((uint) index > (uint) _count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, null);
        }

        var capacity = Capacity;

        if (_count == capacity)
        {
            Array.Resize(ref _items, Math.Max(capacity * 2, MinAllocatedCount));
        }

        Debug.Assert(_items is not null);
        Array.Copy(_items, index, _items, index + 1, Count - index);
        _items![index] = item;
        _count++;

        OnChanged();
    }

    public void RemoveAt(int index)
    {
        AssertUnchanged();

        if ((uint) index >= (uint) _count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, null);
        }

        _count--;

        if (index < _count)
        {
            Array.Copy(_items, index + 1, _items, index, Count - index);
        }

        _items![_count] = default!;

        OnChanged();
    }

    public void Sort(IComparer<T>? comparer = null)
    {
        AssertUnchanged();

        if (_count > 1)
        {
            Array.Sort(_items, 0, _count, comparer);
        }

        OnChanged();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T item)
    {
        Add(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Pop()
    {
        var lastIndex = _count - 1;
        var last = this[lastIndex];
        RemoveAt(lastIndex);
        return last;
    }

    public void Yield(out T[]? items, out int count)
    {
        items = _items;
        count = _count;
        this = default;
    }

    /// <remarks>
    /// Items should not be added or removed from the <see cref="ArrayList{T}"/> while the returned <see cref="Span{T}"/> is in use!
    /// </remarks>
    public Span<T> AsSpan() => new Span<T>(_items, 0, _count);

#if NETSTANDARD2_1_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public T[] ToArray()
    {
#if NETSTANDARD2_1_OR_GREATER
        return AsSpan().ToArray();
#else
        if (_count == 0)
        {
            return Array.Empty<T>();
        }

        var array = new T[_count];
        Array.Copy(_items, 0, array, 0, _count);
        return array;
#endif
    }

    public Enumerator GetEnumerator()
    {
        AssertUnchanged();
        return new Enumerator(_items, _count);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <remarks>
    /// This implementation does not detect changes to the list
    /// during iteration and therefore the behaviour is undefined
    /// under those conditions.
    /// </remarks>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly T[]? _items; // Usually null when count is zero
        private readonly int _count;

        private int _index;
        private T? _current;

        internal Enumerator(T[]? items, int count) : this()
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
                _current = _items![_index];
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

        public T Current => _current!;

        object? IEnumerator.Current
        {
            get
            {
                if (_index == 0 || _index == _count + 1)
                {
                    throw new InvalidOperationException();
                }

                return Current;
            }
        }
    }

#if DEBUG
    [DebuggerNonUserCode]
    private sealed class DebugView
    {
        readonly ArrayList<T> _list;

        public DebugView(ArrayList<T> list)
        {
            _list = list;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                // NOTE: We can't simply call ToArray() because it breaks VS debugger for some reason
                // (probably it's related to the span-based implementation)
                if (_list._count == 0)
                {
                    return Array.Empty<T>();
                }
                var array = new T[_list._count];
                Array.Copy(_list._items, array, _list._count);
                return array;
            }
        }
    }
#endif
}

internal static class ArrayList
{
    public static ArrayList<T> Create<T>(in NodeList<T> source) where T : Node
    {
        var items = source.ToArray();
        return new ArrayList<T>(items);
    }

    public static void AddRange<T>(
        ref this ArrayList<T> destination,
        in NodeList<T> source) where T : Node
    {
        foreach (var item in source)
        {
            destination.Add(item);
        }
    }
}
