using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class NewExpression : Expression
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

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Callee, Arguments);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitNewExpression(this);

    public NewExpression UpdateWith(Expression callee, in NodeList<Expression> arguments)
    {
        if (callee == Callee && NodeList.AreSame(arguments, Arguments))
        {
            return this;
        }

        return new NewExpression(callee, arguments);
    }
}
