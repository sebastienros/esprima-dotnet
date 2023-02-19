using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Elements) })]
public sealed partial class ArrayPattern : BindingPattern
{
    private readonly NodeList<Node?> _elements;

    public ArrayPattern(in NodeList<Node?> elements) : base(Nodes.ArrayPattern)
    {
        _elements = elements;
    }

    /// <summary>
    /// { <see cref="Identifier"/> | <see cref="MemberExpression"/> (in assignment contexts only) | <see cref="BindingPattern"/> | <see cref="AssignmentPattern"/> | <see cref="RestElement"/> | <see langword="null"/> (omitted element) }
    /// </summary>
    public ref readonly NodeList<Node?> Elements { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _elements; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ArrayPattern Rewrite(in NodeList<Node?> elements)
    {
        return new ArrayPattern(elements);
    }
}
