using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast;

public sealed class TemplateLiteral : Expression
{
    private readonly NodeList<TemplateElement> _quasis;
    private readonly NodeList<Expression> _expressions;

    public TemplateLiteral(
        in NodeList<TemplateElement> quasis,
        in NodeList<Expression> expressions)
        : base(Nodes.TemplateLiteral)
    {
        _quasis = quasis;
        _expressions = expressions;
    }

    public ref readonly NodeList<TemplateElement> Quasis { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _quasis; }
    public ref readonly NodeList<Expression> Expressions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _expressions; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextTemplateLiteral(Quasis, Expressions);

    protected internal override T Accept<T>(AstVisitor<T> visitor) => visitor.VisitTemplateLiteral(this);

    public TemplateLiteral UpdateWith(in NodeList<TemplateElement> quasis, in NodeList<Expression> expressions)
    {
        if (NodeList.AreSame(quasis, Quasis) && NodeList.AreSame(expressions, Expressions))
        {
            return this;
        }

        return new TemplateLiteral(quasis, expressions);
    }
}
