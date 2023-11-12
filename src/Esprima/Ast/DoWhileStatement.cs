using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Body), nameof(Test) })]
public sealed partial class DoWhileStatement : Statement
{
    public DoWhileStatement(Statement body, Expression test) : base(Nodes.DoWhileStatement)
    {
        Body = body;
        Test = test;
    }

    public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DoWhileStatement Rewrite(Statement body, Expression test)
    {
        return new DoWhileStatement(body, test);
    }
}
