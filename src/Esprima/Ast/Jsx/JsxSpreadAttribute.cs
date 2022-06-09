using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxSpreadAttribute : JsxExpression
{
    public readonly Expression Argument;

    public JsxSpreadAttribute(Expression argument) : base(JsxNodeType.SpreadAttribute)
    {
        Argument = argument;
    }

    public override NodeCollection ChildNodes => new(Argument);

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxSpreadAttribute(this);
    }

    public JsxSpreadAttribute UpdateWith(Expression argument)
    {
        if (argument == Argument)
        {
            return this;
        }

        return new JsxSpreadAttribute(argument).SetAdditionalInfo(this);
    }
}
