using System;

namespace Esprima.Tests
{
    internal static class Lazy
    {
        public static Lazy<T, TMetadata>
            Create<T, TMetadata>(TMetadata metadata, Func<T> factory) =>
            new Lazy<T, TMetadata>(factory, metadata);
    }

    public sealed class Lazy<T, TMetadata> : Lazy<T>
    {
        public TMetadata Metadata { get; }

        public Lazy(Func<T> valueFactory, TMetadata metadata) :
            base(valueFactory)
        {
            Metadata = metadata;
        }

        public override string ToString() => $"{Metadata}";
    }
}