using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Esprima.EsprimaExceptionHelper;

namespace Esprima.Ast;

public readonly struct NodeList<T> : IReadOnlyList<T> where T : Node?
{
    private readonly T[]? _items;
    private readonly int _count;

    internal NodeList(ICollection<T> collection)
    {
        if (collection is null)
        {
            ThrowArgumentNullException();
        }

        _count = collection!.Count;
        if (_count > 0)
        {
            _items = new T[_count];
            collection.CopyTo(_items, 0);
        }

        static void ThrowArgumentNullException()
        {
            ThrowArgumentNullException<T>(nameof(collection));
        }
    }

    /// <remarks>
    /// Expects ownership of the array!
    /// </remarks>
    internal NodeList(T[]? items, int count)
    {
        Debug.Assert(count <= (items?.Length ?? 0));

        _items = items;
        _count = count;
    }

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            // Following trick can reduce the range check by one
            if ((uint) index < (uint) _count)
            {
                return _items![index];
            }

            return ThrowIndexOutOfRangeException<T>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool IsSameAs(in NodeList<T> other) => ReferenceEquals(_items, other._items);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NodeList<Node?> AsNodes()
    {
        return new NodeList<Node?>(_items /* conversion by co-variance! */, _count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NodeList<TTo> As<TTo>() where TTo : Node?
    {
        return new NodeList<TTo>((TTo[]?) (object?) _items, _count);
    }

    public ReadOnlySpan<T> AsSpan() => new ReadOnlySpan<T>(_items, 0, _count);

    public ReadOnlyMemory<T> AsMemory() => new ReadOnlyMemory<T>(_items, 0, _count);

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
        return new Enumerator(_items, Count);
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
}

public static class NodeList
{
    internal static NodeList<T> From<T>(ref ArrayList<T> arrayList) where T : Node?
    {
        arrayList.Yield(out var items, out var count);
        return new NodeList<T>(items, count);
    }

    public static NodeList<T> Create<T>(IEnumerable<T> source) where T : Node?
    {
        switch (source)
        {
            case null:
                {
                    throw new ArgumentNullException(nameof(source));
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
                    var count = source is IReadOnlyCollection<T> collection
                        ? collection.Count
                        : (int?) null;

                    var list = count is int initialCapacity
                        ? new ArrayList<T>(initialCapacity)
                        : new ArrayList<T>();

                    if (count is null || count > 0)
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
