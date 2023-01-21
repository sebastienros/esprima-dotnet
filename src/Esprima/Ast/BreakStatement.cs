using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Label) })]
public sealed class BreakStatement : Statement
{
    public BreakStatement(Identifier? label) : base(Nodes.BreakStatement)
    {
        Label = label;
    }

    public Identifier? Label { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullable(Label);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitBreakStatement(this);

    public BreakStatement UpdateWith(Identifier? label)
    {
        if (label == Label)
        {
            return this;
        }

        return new BreakStatement(label);
    }
}
