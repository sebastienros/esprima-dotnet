using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class ContinueStatement : Statement
{
    public ContinueStatement(Identifier? label) : base(Nodes.ContinueStatement)
    {
        Label = label;
    }

    public Identifier? Label { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullable(Label);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitContinueStatement(this);

    public ContinueStatement UpdateWith(Identifier? label)
    {
        if (label == Label)
        {
            return this;
        }

        return new ContinueStatement(label);
    }
}
