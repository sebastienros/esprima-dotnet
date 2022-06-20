using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class PrivateIdentifier : Expression
    {
        public PrivateIdentifier(string? name) : base(Nodes.PrivateIdentifier)
        {
            Name = name;
        }

        public string? Name { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitPrivateIdentifier(this, context);
        }
    }
}
