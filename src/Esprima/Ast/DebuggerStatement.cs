using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class DebuggerStatement : Statement
    {
        public DebuggerStatement() : base(Nodes.DebuggerStatement) { }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitDebuggerStatement(this) as T;
        }
    }
}
