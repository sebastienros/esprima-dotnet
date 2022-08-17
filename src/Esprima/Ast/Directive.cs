using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;

namespace Esprima.Ast;

public sealed class Directive : ExpressionStatement
{
    public Directive(Expression expression, StringSegment? value) : base(expression)
    {
        Value = value;
    }

    public StringSegment? Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    protected override ExpressionStatement Rewrite(Expression expression)
    {
        return new Directive(expression, Value);
    }
}
