using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Label) })]
public sealed partial class BreakStatement : Statement
{
    public BreakStatement(Identifier? label) : base(Nodes.BreakStatement)
    {
        Label = label;
    }

    public Identifier? Label { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BreakStatement Rewrite(Identifier? label)
    {
        return new BreakStatement(label);
    }
}
