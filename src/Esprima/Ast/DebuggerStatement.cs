using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class DebuggerStatement : Statement
    {
        public DebuggerStatement() : base(Nodes.DebuggerStatement) { }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override Node? Accept(AstVisitor visitor)
        {
            return visitor.VisitDebuggerStatement(this);
        }
    }
}
