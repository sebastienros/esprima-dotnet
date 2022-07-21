﻿using System;

namespace Esprima.Tests.Helpers;

internal static class Lazy
{
    public static Lazy<T, TMetadata>
        Create<T, TMetadata>(TMetadata metadata, Func<T> factory)
    {
        return new Lazy<T, TMetadata>(factory, metadata);
    }
}

public sealed class Lazy<T, TMetadata> : Lazy<T>
{
    public TMetadata Metadata { get; }

    public Lazy(Func<T> valueFactory, TMetadata metadata) :
        base(valueFactory)
    {
        Metadata = metadata;
    }

    public override string ToString()
    {
        return $"{Metadata}";
    }
}
