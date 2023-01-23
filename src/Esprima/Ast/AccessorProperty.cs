using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Decorators), nameof(Key), nameof(Value) })]
public sealed partial class AccessorProperty : ClassProperty
{
    private readonly NodeList<Decorator> _decorators;

    public AccessorProperty(
        Expression key,
        bool computed,
        Expression? value,
        bool isStatic,
        in NodeList<Decorator> decorators)
        : base(Nodes.AccessorProperty, PropertyKind.Property, key, computed)
    {
        Value = value;
        Static = isStatic;
        _decorators = decorators;
    }

    public new Expression? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    protected override Expression? GetValue() => Value;

    public bool Static { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<Decorator> Decorators { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _decorators; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private AccessorProperty Rewrite(in NodeList<Decorator> decorators, Expression key, Expression? value)
    {
        return new AccessorProperty(key, Computed, value, Static, decorators);
    }
}
