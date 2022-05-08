﻿using System.Diagnostics;
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

    protected internal override T? Accept<T>(AstVisitor visitor) where T : class
    {
        return visitor.VisitJsxNamespacedName(this) as T;
    }
}
