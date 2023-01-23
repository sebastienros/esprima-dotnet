using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Left), nameof(Right), nameof(Body) })]
public sealed partial class ForOfStatement : Statement
{
    public ForOfStatement(
        Node left,
        Expression right,
        Statement body,
        bool await) : base(Nodes.ForOfStatement)
    {
        Left = left;
        Right = right;
        Body = body;
        Await = await;
    }

    /// <remarks>
    /// <see cref="VariableDeclaration"/> (cannot have an initializer) | <see cref="Identifier"/> | <see cref="MemberExpression"/> | <see cref="BindingPattern"/>
    /// </remarks>
    public Node Left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public bool Await { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ForOfStatement Rewrite(Node left, Expression right, Statement body)
    {
        return new ForOfStatement(left, right, body, Await);
    }
}
