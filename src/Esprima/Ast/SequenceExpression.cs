using System.Runtime.CompilerServices;

namespace Esprima.Ast;

[VisitableNode(ChildProperties = new[] { nameof(Expressions) })]
public sealed partial class SequenceExpression : Expression
{
    private readonly NodeList<Expression> _expressions;

    public SequenceExpression(in NodeList<Expression> expressions) : base(Nodes.SequenceExpression)
    {
        _expressions = expressions;
    }

    public ref readonly NodeList<Expression> Expressions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _expressions; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SequenceExpression Rewrite(in NodeList<Expression> expressions)
    {
        return new SequenceExpression(expressions);
    }
}
