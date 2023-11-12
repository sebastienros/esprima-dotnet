using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Id), nameof(Init) })]
public sealed partial class VariableDeclarator : Node
{
    public VariableDeclarator(Node id, Expression? init) :
        base(Nodes.VariableDeclarator)
    {
        Id = id;
        Init = init;
    }

    /// <remarks>
    /// <see cref="Identifier"/> | <see cref="BindingPattern"/>
    /// </remarks>
    public Node Id { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? Init { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static VariableDeclarator Rewrite(Node id, Expression? init)
    {
        return new VariableDeclarator(id, init);
    }
}
