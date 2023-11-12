using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Label) })]
public sealed partial class ContinueStatement : Statement
{
    public ContinueStatement(Identifier? label) : base(Nodes.ContinueStatement)
    {
        Label = label;
    }

    public Identifier? Label { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ContinueStatement Rewrite(Identifier? label)
    {
        return new ContinueStatement(label);
    }
}
