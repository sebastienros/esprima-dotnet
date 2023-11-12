using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Discriminant), nameof(Cases) })]
public sealed partial class SwitchStatement : Statement
{
    private readonly NodeList<SwitchCase> _cases;

    public SwitchStatement(Expression discriminant, in NodeList<SwitchCase> cases) : base(Nodes.SwitchStatement)
    {
        Discriminant = discriminant;
        _cases = cases;
    }

    public Expression Discriminant { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<SwitchCase> Cases { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _cases; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SwitchStatement Rewrite(Expression discriminant, in NodeList<SwitchCase> cases)
    {
        return new SwitchStatement(discriminant, cases);
    }
}
