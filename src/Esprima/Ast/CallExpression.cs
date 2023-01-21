using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Callee), nameof(Arguments) })]
public sealed class CallExpression : Expression
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

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Callee, Arguments);

    protected internal override object? Accept(AstVisitor visitor) => visitor.VisitCallExpression(this);

    public CallExpression UpdateWith(Expression callee, in NodeList<Expression> arguments)
    {
        if (callee == Callee && NodeList.AreSame(arguments, Arguments))
        {
            return this;
        }

        return new CallExpression(callee, arguments, Optional);
    }
}
