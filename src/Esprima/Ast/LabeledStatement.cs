using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class LabeledStatement : Statement
{
    public LabeledStatement(Identifier label, Statement body) : base(Nodes.LabeledStatement)
    {
        Label = label;
        Body = body;
        body._labelSet = label;
    }

    public Identifier Label { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Label, Body);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitLabeledStatement(this);

    public LabeledStatement UpdateWith(Identifier label, Statement body)
    {
        if (label == Label && body == Body)
        {
            return this;
        }

        return new LabeledStatement(label, body);
    }
}
