using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Test), nameof(Consequent) })]
public sealed partial class SwitchCase : Node
{
    private readonly NodeList<Statement> _consequent;

    public SwitchCase(Expression? test, in NodeList<Statement> consequent) : base(Nodes.SwitchCase)
    {
        Test = test;
        _consequent = consequent;
    }

    public Expression? Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<Statement> Consequent { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _consequent; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SwitchCase Rewrite(Expression? test, in NodeList<Statement> consequent)
    {
        return new SwitchCase(test, consequent);
    }
}
