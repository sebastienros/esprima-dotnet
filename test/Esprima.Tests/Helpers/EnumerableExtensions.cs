using System.Collections;

namespace Esprima.Tests.Helpers;

public static class EnumerableExtensions
{
    public static bool SequenceEqualUnordered<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> second)
    {
        return SequenceEqualUnordered(source, second, EqualityComparer<TSource>.Default);
    }

    public static bool SequenceEqualUnordered<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (second == null)
            throw new ArgumentNullException(nameof(second));

        if (source.TryGetCount(out int firstCount) && second.TryGetCount(out int secondCount))
        {
            if (firstCount != secondCount)
                return false;

            if (firstCount == 0)
                return true;
        }

        IEqualityComparer<ValueTuple<TSource>>? wrapperComparer = comparer != null ? new WrappedItemComparer<TSource>(comparer) : null;

        Dictionary<ValueTuple<TSource>, int> counters;
        ValueTuple<TSource> key;
        int counter;

        using (IEnumerator<TSource> enumerator = source.GetEnumerator())
        {
            if (!enumerator.MoveNext())
                return !second.Any();

            counters = new Dictionary<ValueTuple<TSource>, int>(wrapperComparer);

            do
            {
                key = new ValueTuple<TSource>(enumerator.Current);

                if (counters.TryGetValue(key, out counter))
                    counters[key] = counter + 1;
                else
                    counters.Add(key, 1);
            }
            while (enumerator.MoveNext());
        }

        foreach (TSource item in second)
        {
            key = new ValueTuple<TSource>(item);

            if (counters.TryGetValue(key, out counter))
            {
                if (counter <= 0)
                    return false;

                counters[key] = counter - 1;
            }
            else
                return false;
        }

        return counters.Values.All(cnt => cnt == 0);
    }

    private static bool TryGetCount<TSource>(this IEnumerable<TSource> source, out int count)
    {
        switch (source)
        {
            case ICollection<TSource> collection:
                count = collection.Count;
                return true;
            case IReadOnlyCollection<TSource> readOnlyCollection:
                count = readOnlyCollection.Count;
                return true;
            case ICollection nonGenericCollection:
                count = nonGenericCollection.Count;
                return true;
            default:
                count = default;
                return false;
        }
    }

    private sealed class WrappedItemComparer<TSource> : IEqualityComparer<ValueTuple<TSource>>
    {
        private readonly IEqualityComparer<TSource> _comparer;

        public WrappedItemComparer(IEqualityComparer<TSource> comparer)
        {
            _comparer = comparer;
        }

        public bool Equals(ValueTuple<TSource> x, ValueTuple<TSource> y) => _comparer.Equals(x.Item1, y.Item1);

        public int GetHashCode(ValueTuple<TSource> obj) => _comparer.GetHashCode(obj.Item1!);
    }
}
