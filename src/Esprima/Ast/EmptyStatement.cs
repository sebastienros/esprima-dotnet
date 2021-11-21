using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class EmptyStatement : Statement
    {
        public EmptyStatement() : base(Nodes.EmptyStatement)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override Node? Accept(AstVisitor visitor)
        {
            return visitor.VisitEmptyStatement(this);
        }
    }
}
