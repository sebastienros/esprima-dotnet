using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Decorators), nameof(Key), nameof(Value) })]
public sealed partial class MethodDefinition : ClassProperty
{
    private readonly NodeList<Decorator> _decorators;

    public MethodDefinition(
        Expression key,
        bool computed,
        FunctionExpression value,
        PropertyKind kind,
        bool isStatic,
        in NodeList<Decorator> decorators)
        : base(Nodes.MethodDefinition, kind, key, computed)
    {
        Value = value;
        Static = isStatic;
        _decorators = decorators;
    }

    public new FunctionExpression Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    protected override Expression? GetValue() => Value;

    public bool Static { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<Decorator> Decorators { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _decorators; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MethodDefinition Rewrite(in NodeList<Decorator> decorators, Expression key, FunctionExpression value)
    {
        return new MethodDefinition(key, Computed, value, Kind, Static, decorators);
    }
}
