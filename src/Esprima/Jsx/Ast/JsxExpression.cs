using Esprima.Ast;

namespace Esprima.Jsx.Ast;

/// <summary>
/// A Jsx expression.
/// </summary>
public abstract class JsxExpression : Expression
{
    public new readonly JsxNodeType Type;

    protected JsxExpression(JsxNodeType type) : base(Nodes.Extension)
    {
        Type = type;
    }
}
