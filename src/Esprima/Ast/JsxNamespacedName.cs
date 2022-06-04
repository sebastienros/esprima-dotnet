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

    protected internal override object? Accept(AstVisitor visitor)
    {
        return visitor.VisitJsxNamespacedName(this);
    }

    public JsxNamespacedName UpdateWith(JsxIdentifier name, JsxIdentifier @namespace)
    {
        if (name == Name && @namespace == Namespace)
        {
            return this;
        }

        return new JsxNamespacedName(@namespace, name);
    }
}
