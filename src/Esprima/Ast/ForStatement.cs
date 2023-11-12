using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Init), nameof(Test), nameof(Update), nameof(Body) })]
public sealed partial class ForStatement : Statement
{
    public ForStatement(
        StatementListItem? init,
        Expression? test,
        Expression? update,
        Statement body)
        : base(Nodes.ForStatement)
    {
        Init = init;
        Test = test;
        Update = update;
        Body = body;
    }

    /// <remarks>
    /// <see cref="VariableDeclaration"/> (var i) | <see cref="Expression"/> (i=0)
    /// </remarks>
    public StatementListItem? Init { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression? Update { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ForStatement Rewrite(StatementListItem? init, Expression? test, Expression? update, Statement body)
    {
        return new ForStatement(init, test, update, body);
    }
}
