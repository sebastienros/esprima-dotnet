using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxSpreadAttribute : JsxExpression
{
    public JsxSpreadAttribute(Expression argument) : base(JsxNodeType.SpreadAttribute)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Argument);

    protected internal override T Accept<T>(IJsxAstVisitor<T> visitor) => visitor.VisitJsxSpreadAttribute(this);

    public JsxSpreadAttribute UpdateWith(Expression argument)
    {
        if (argument == Argument)
        {
            return this;
        }

        return new JsxSpreadAttribute(argument);
    }
}
