using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class AccessorProperty : Node
{
    private readonly NodeList<Decorator> _decorators;

    public AccessorProperty(Expression key, Expression? value, bool computed, bool isStatic, in NodeList<Decorator> decorators) : base(Nodes.AccessorProperty)
    {
        Key = key;
        Value = value;
        Computed = computed;
        Static = isStatic;
        _decorators = decorators;
    }

    /// <remarks>
    /// <see cref="Expression" /> | <see cref="PrivateIdentifier" />
    /// </remarks>
    public Expression Key { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Computed { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Static { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<Decorator> Decorators { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _decorators; }

    public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

    private IEnumerable<Node> CreateChildNodes()
    {
        yield return Key;

        if (Value is not null)
        {
            yield return Value;
        }

        foreach (var node in Decorators)
        {
            yield return node;
        }
    }

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitAccessorProperty(this);
    }

    public AccessorProperty UpdateWith(Expression key, Expression? value, in NodeList<Decorator> decorators)
    {
        if (key == Key && value == Value && NodeList.AreSame(decorators, Decorators))
        {
            return this;
        }

        return new AccessorProperty(key, value, Computed, Static, decorators).SetAdditionalInfo(this);
    }
}
