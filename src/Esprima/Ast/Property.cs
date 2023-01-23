using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Key), nameof(Value) })]
public sealed partial class Property : Node, IProperty
{
    internal Node _value;

    public Property(
        PropertyKind kind,
        Expression key,
        bool computed,
        Node value,
        bool method,
        bool shorthand)
        : base(Nodes.Property)
    {
        Kind = kind;
        Key = key;
        Computed = computed;
        _value = value;
        Method = method;
        Shorthand = shorthand;
    }

    public PropertyKind Kind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="Literal"/> (string or numeric) | '[' <see cref="Expression"/> ']'
    /// </remarks>
    public Expression Key { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Computed { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    /// <remarks>
    /// When property of an object literal: <see cref="Expression"/> (incl. <see cref="SpreadElement"/> and <see cref="FunctionExpression"/> for getters/setters/methods) <br />
    /// When property of an object binding pattern: <see cref="Identifier"/> | <see cref="MemberExpression"/> (in assignment contexts only) | <see cref="BindingPattern"/> | <see cref="AssignmentPattern"/> | <see cref="RestElement"/>
    /// </remarks>
    public Node Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value; }
    Node? IProperty.Value => Value;

    public bool Method { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Shorthand { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextProperty(Key, Value, Shorthand);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Property Rewrite(Expression key, Node value)
    {
        return new Property(Kind, key, Computed, value, Method, Shorthand);
    }
}
