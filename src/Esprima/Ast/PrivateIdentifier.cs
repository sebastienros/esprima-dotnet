using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class PrivateIdentifier : Expression
    {
        public readonly string? Name;

        public PrivateIdentifier(string? name) : base(Nodes.PrivateIdentifier)
        {
            Name = name;
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override Node? Accept(AstVisitor visitor)
        {
            return visitor.VisitPrivateIdentifier(this);
        }
    }
}
