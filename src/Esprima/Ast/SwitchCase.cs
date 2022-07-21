using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class SwitchCase : Node
{
    private readonly NodeList<Statement> _consequent;

    public SwitchCase(Expression? test, in NodeList<Statement> consequent) : base(Nodes.SwitchCase)
    {
        Test = test;
        _consequent = consequent;
    }

    public Expression? Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<Statement> Consequent { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _consequent; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextNullableAt0(Test, Consequent);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitSwitchCase(this);

    public SwitchCase UpdateWith(Expression? test, in NodeList<Statement> consequent)
    {
        if (test == Test && NodeList.AreSame(consequent, Consequent))
        {
            return this;
        }

        return new SwitchCase(test, consequent);
    }
}
