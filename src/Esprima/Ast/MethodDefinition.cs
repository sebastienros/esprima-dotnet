using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class MethodDefinition : ClassProperty
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

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Decorators, Key, Value);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitMethodDefinition(this);

    public MethodDefinition UpdateWith(Expression key, FunctionExpression value, in NodeList<Decorator> decorators)
    {
        if (key == Key && value == Value && NodeList.AreSame(decorators, Decorators))
        {
            return this;
        }

        return new MethodDefinition(key, Computed, value, Kind, Static, decorators);
    }
}
