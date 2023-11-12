using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Callee), nameof(Arguments) })]
public sealed partial class NewExpression : Expression
{
    private readonly NodeList<Expression> _arguments;

    public NewExpression(
        Expression callee,
        in NodeList<Expression> args)
        : base(Nodes.NewExpression)
    {
        Callee = callee;
        _arguments = args;
    }

    public Expression Callee { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public ref readonly NodeList<Expression> Arguments { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _arguments; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static NewExpression Rewrite(Expression callee, in NodeList<Expression> arguments)
    {
        return new NewExpression(callee, arguments);
    }
}
