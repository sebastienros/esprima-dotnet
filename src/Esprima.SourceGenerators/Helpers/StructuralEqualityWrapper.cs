using System.Collections;

namespace Esprima.SourceGenerators.Helpers;

internal readonly struct StructuralEqualityWrapper<TTarget> : IEquatable<StructuralEqualityWrapper<TTarget>>
    where TTarget : IStructuralEquatable
{
    public static implicit operator StructuralEqualityWrapper<TTarget>(TTarget target) => new StructuralEqualityWrapper<TTarget>(target);

    public StructuralEqualityWrapper(TTarget target)
    {
        Target = target;
    }

    public TTarget Target { get; }

    public override bool Equals(object? obj) => obj is StructuralEqualityWrapper<TTarget> wrapper && Equals(wrapper);

    public bool Equals(StructuralEqualityWrapper<TTarget> other) => StructuralComparisons.StructuralEqualityComparer.Equals(Target, other.Target);

    public override int GetHashCode() => StructuralComparisons.StructuralEqualityComparer.GetHashCode(Target);

    public override string ToString() => Target?.ToString() ?? string.Empty;
}
