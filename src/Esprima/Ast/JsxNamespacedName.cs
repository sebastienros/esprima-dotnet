using System.Diagnostics;
using Esprima.Utils;

namespace Esprima.Ast;

[DebuggerDisplay("{Namespace,nq}.{Name,nq}")]
public sealed class JsxNamespacedName : JsxExpression
{
    public readonly JsxIdentifier Name;
    public readonly JsxIdentifier Namespace;

    public JsxNamespacedName(JsxIdentifier @namespace,JsxIdentifier name) : base(Nodes.JSXNamespacedName)
    {
        Name = name;
        Namespace = @namespace;
    }

    public override NodeCollection ChildNodes => new(Name, Namespace);

    protected internal override void Accept(AstVisitor visitor)
    {
        visitor.VisitJsxNamespacedName(this);
    }
}