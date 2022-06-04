using System.Diagnostics;
using Esprima.Utils;

namespace Esprima.Ast
{
    [DebuggerDisplay("{Name,nq}")]
    public sealed class Identifier : Expression
    {
        public readonly string? Name;

        public Identifier(string? name) : base(Nodes.Identifier)
        {
            Name = name;
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitIdentifier(this);
        }
    }
}
