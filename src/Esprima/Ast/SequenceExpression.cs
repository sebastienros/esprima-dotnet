using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class SequenceExpression : Expression
{
    private readonly NodeList<Expression> _expressions;

    public SequenceExpression(in NodeList<Expression> expressions) : base(Nodes.SequenceExpression)
    {
        _expressions = expressions;
    }

    public ref readonly NodeList<Expression> Expressions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _expressions; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Expressions);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitSequenceExpression(this);

    public SequenceExpression UpdateWith(in NodeList<Expression> expressions)
    {
        if (NodeList.AreSame(expressions, Expressions))
        {
            return this;
        }

        return new SequenceExpression(expressions);
    }
}
