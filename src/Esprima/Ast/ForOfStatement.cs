using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Left), nameof(Right), nameof(Body) })]
public sealed class ForOfStatement : Statement
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

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Left, Right, Body);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitForOfStatement(this);

    public ForOfStatement UpdateWith(Node left, Expression right, Statement body)
    {
        if (left == Left && right == Right && body == Body)
        {
            return this;
        }

        return new ForOfStatement(left, right, body, Await);
    }
}
