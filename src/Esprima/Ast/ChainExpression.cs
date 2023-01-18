using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class ChainExpression : Expression
{
    public ChainExpression(Expression expression) : base(Nodes.ChainExpression)
    {
        Expression = expression;
    }

    /// <remarks>
    /// <see cref="CallExpression"/> | <see cref="ComputedMemberExpression"/>| <see cref="StaticMemberExpression"/>
    /// </remarks>
    public Expression Expression { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Expression);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitChainExpression(this);

    public ChainExpression UpdateWith(Expression expression)
    {
        if (expression == Expression)
        {
            return this;
        }

        return new ChainExpression(expression);
    }
}
