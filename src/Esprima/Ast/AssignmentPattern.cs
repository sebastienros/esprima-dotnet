using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Left), nameof(Right) })]
public sealed partial class AssignmentPattern : Node
{
    internal Expression _right;

    public AssignmentPattern(Node left, Expression right) : base(Nodes.AssignmentPattern)
    {
        Left = left;
        _right = right;
    }

    /// <summary>
    /// <see cref="Identifier"/> | <see cref="MemberExpression"/> (in assignment contexts only) | <see cref="BindingPattern"/>
    /// </summary>
    public Node Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _right; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AssignmentPattern Rewrite(Node left, Expression right)
    {
        return new AssignmentPattern(left, right);
    }
}
