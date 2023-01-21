using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Body), nameof(Test) })]
public sealed class DoWhileStatement : Statement
{
    public DoWhileStatement(Statement body, Expression test) : base(Nodes.DoWhileStatement)
    {
        Body = body;
        Test = test;
    }

    public Statement Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Expression Test { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Body, Test);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitDoWhileStatement(this);

    public DoWhileStatement UpdateWith(Statement body, Expression test)
    {
        if (body == Body && test == Test)
        {
            return this;
        }

        return new DoWhileStatement(body, test);
    }
}
