using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Object), nameof(Body) })]
public sealed partial class WithStatement : Statement
{
    public WithStatement(Expression obj, Statement body) : base(Nodes.WithStatement)
    {
        Object = obj;
        Body = body;
    }

    public Expression Object { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private WithStatement Rewrite(Expression @object, Statement body)
    {
        return new WithStatement(@object, body);
    }
}
