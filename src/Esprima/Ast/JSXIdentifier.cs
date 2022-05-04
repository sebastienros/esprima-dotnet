using System.Diagnostics;
using Esprima.Utils;

namespace Esprima.Ast
{
    [DebuggerDisplay("{Name,nq}")]
    public sealed class JSXIdentifier : JSXExpression
    {
        public readonly string Name;

        public JSXIdentifier(string name) : base(Nodes.JSXIdentifier)
        {
            Name = name;
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXIdentifier(this);
        }
    }
}
