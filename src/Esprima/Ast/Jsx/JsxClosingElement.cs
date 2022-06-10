using System.Runtime.CompilerServices;
using Esprima.Utils.Jsx;

namespace Esprima.Ast.Jsx;

public sealed class JsxClosingElement : JsxExpression
{
    public JsxClosingElement(JsxExpression name) : base(JsxNodeType.ClosingElement)
    {
        Name = name;
    }

    public JsxExpression Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

    public override NodeCollection ChildNodes => new(Name);

    protected override object? Accept(IJsxAstVisitor visitor)
    {
        return visitor.VisitJsxClosingElement(this);
    }

    public JsxClosingElement UpdateWith(JsxExpression name)
    {
        if (name == Name)
        {
            return this;
        }

        return new JsxClosingElement(name).SetAdditionalInfo(this);
    }
}
