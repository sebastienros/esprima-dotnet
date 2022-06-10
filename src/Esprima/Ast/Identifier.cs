using System.Diagnostics;
using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    [DebuggerDisplay("{Name,nq}")]
    public sealed class Identifier : Expression
    {
        public Identifier(string? name) : base(Nodes.Identifier)
        {
            Name = name;
        }

        public string? Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitIdentifier(this);
        }
    }
}
