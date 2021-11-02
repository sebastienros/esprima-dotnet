using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class EmptyStatement : Statement
    {
        public EmptyStatement() : base(Nodes.EmptyStatement)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitEmptyStatement(this);
        }
    }
}
