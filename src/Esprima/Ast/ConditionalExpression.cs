using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Test), nameof(Consequent), nameof(Alternate) })]
public sealed class ConditionalExpression : Expression
{
    public ConditionalExpression(
        Expression test,
        Expression consequent,
        Expression alternate) : base(Nodes.ConditionalExpression)
    {
        Test = test;
        Consequent = consequent;
        Alternate = alternate;
    }

    public Expression Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Consequent { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Alternate { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Test, Consequent, Alternate);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitConditionalExpression(this);

    public ConditionalExpression UpdateWith(Expression test, Expression consequent, Expression alternate)
    {
        if (test == Test && consequent == Consequent && alternate == Alternate)
        {
            return this;
        }

        return new ConditionalExpression(test, consequent, alternate);
    }
}
