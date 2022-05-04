using System.Diagnostics;
using Esprima.Utils;

namespace Esprima.Ast
{
    [DebuggerDisplay("{Namespace,nq}.{Name,nq}")]
    public sealed class JSXNamespacedName : JSXExpression
    {
        public readonly JSXIdentifier Name;
        public readonly JSXIdentifier Namespace;

        public JSXNamespacedName(JSXIdentifier @namespace,JSXIdentifier name) : base(Nodes.JSXNamespacedName)
        {
            Name = name;
            Namespace = @namespace;
        }

        public override NodeCollection ChildNodes => new(Name, Namespace);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXNamespacedName(this);
        }
    }
}
