namespace Esprima.Ast;

/// <summary>
/// A Jsx expression. 
/// </summary>
public abstract class JsxExpression : Expression
{
    protected JsxExpression(Nodes type) : base(type)
    {
    }
}