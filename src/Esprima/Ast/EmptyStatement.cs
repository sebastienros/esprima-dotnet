using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class EmptyStatement : Statement
    {
        public EmptyStatement() : base(Nodes.EmptyStatement)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitEmptyStatement(this, context);
        }
    }
}
