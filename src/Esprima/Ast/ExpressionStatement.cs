using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Expression) }, SealOverrideMethods = true)]
public partial class ExpressionStatement : Statement
{
    public ExpressionStatement(Expression expression) : base(Nodes.ExpressionStatement)
    {
        Expression = expression;
    }

    public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected virtual ExpressionStatement Rewrite(Expression expression)
    {
        return new ExpressionStatement(expression);
    }
}
