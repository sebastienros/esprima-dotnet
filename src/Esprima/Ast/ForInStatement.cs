using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Left), nameof(Right), nameof(Body) })]
public sealed partial class ForInStatement : Statement
{
    public ForInStatement(
        Node left,
        Expression right,
        Statement body) : base(Nodes.ForInStatement)
    {
        Left = left;
        Right = right;
        Body = body;
    }

    /// <remarks>
    /// <see cref="VariableDeclaration"/> (may have an initializer in non-strict mode) | <see cref="Identifier"/> | <see cref="MemberExpression"/> | <see cref="BindingPattern"/>
    /// </remarks>
    public Node Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ForInStatement Rewrite(Node left, Expression right, Statement body)
    {
        return new ForInStatement(left, right, body);
    }
}
