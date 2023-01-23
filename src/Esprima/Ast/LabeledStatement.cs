using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Label), nameof(Body) })]
public sealed partial class LabeledStatement : Statement
{
    public LabeledStatement(Identifier label, Statement body) : base(Nodes.LabeledStatement)
    {
        Label = label;
        Body = body;
        body._labelSet = label;
    }

    public Identifier Label { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LabeledStatement Rewrite(Identifier label, Statement body)
    {
        return new LabeledStatement(label, body);
    }
}
