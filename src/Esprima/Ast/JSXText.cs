using System.Diagnostics;
using Esprima.Utils;

namespace Esprima.Ast
{
    [DebuggerDisplay("{Raw,nq}")]
    public class JSXText : JSXExpression
    {
        public readonly string? Value;
        public readonly string Raw;
        
        public JSXText(string? value, string raw) : base(Nodes.JSXText)
        { 
            Value = value;
            Raw = raw;
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitJSXText(this);
        }
    }
}
