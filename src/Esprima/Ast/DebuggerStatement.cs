using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class DebuggerStatement: Statement
    {
        public DebuggerStatement() : base(Nodes.DebuggerStatement) {}

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        public override void Accept(AstVisitor visitor) => visitor.VisitDebuggerStatement(this);
    }
}