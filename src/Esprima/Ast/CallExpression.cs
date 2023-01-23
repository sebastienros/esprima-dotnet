using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Callee), nameof(Arguments) })]
public sealed partial class CallExpression : Expression
{
    private readonly NodeList<Expression> _arguments;

    public CallExpression(
        Expression callee,
        in NodeList<Expression> args,
        bool optional) : base(Nodes.CallExpression)
    {
        Callee = callee;
        _arguments = args;
        Optional = optional;
    }

    public Expression Callee { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<Expression> Arguments { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _arguments; }
    public bool Optional { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private CallExpression Rewrite(Expression callee, in NodeList<Expression> arguments)
    {
        return new CallExpression(callee, arguments, Optional);
    }
}
