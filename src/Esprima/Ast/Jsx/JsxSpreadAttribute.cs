using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

[VisitableNode(VisitorType = typeof(IJsxAstVisitor), ChildProperties = new[] { nameof(Argument) })]
public sealed partial class JsxSpreadAttribute : JsxExpression
{
    public JsxSpreadAttribute(Expression argument) : base(JsxNodeType.SpreadAttribute)
    {
        Argument = argument;
    }

    public Expression Argument { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private JsxSpreadAttribute Rewrite(Expression argument)
    {
        return new JsxSpreadAttribute(argument);
    }
}
