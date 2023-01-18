using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public class ExpressionStatement : Statement
{
    public ExpressionStatement(Expression expression) : base(Nodes.ExpressionStatement)
    {
        Expression = expression;
    }

    public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal sealed override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Expression);

    protected internal sealed override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitExpressionStatement(this);

    protected virtual ExpressionStatement Rewrite(Expression expression)
    {
        return new ExpressionStatement(expression);
    }

    public ExpressionStatement UpdateWith(Expression expression)
    {
        if (expression == Expression)
        {
            return this;
        }

        return Rewrite(expression);
    }
}
