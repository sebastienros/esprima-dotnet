using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Elements) })]
public sealed partial class ArrayExpression : Expression
{
    private readonly NodeList<Expression?> _elements;

    public ArrayExpression(in NodeList<Expression?> elements) : base(Nodes.ArrayExpression)
    {
        _elements = elements;
    }

    /// <summary>
    /// { <see cref="Expression"/> (incl. <see cref="SpreadElement"/>) | <see langword="null"/> (omitted element) }
    /// </summary>
    public ref readonly NodeList<Expression?> Elements { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _elements; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ArrayExpression Rewrite(in NodeList<Expression?> elements)
    {
        return new ArrayExpression(elements);
    }
}
