using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class CatchClause : Statement
    {
        public readonly Expression? Param; // BindingIdentifier | BindingPattern | null;
        public readonly BlockStatement Body;

        public CatchClause(Expression? param, BlockStatement body) :
            base(Nodes.CatchClause)
        {
            Param = param;
            Body = body;
        }

        public override NodeCollection ChildNodes => new(Param, Body);

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitCatchClause(this) as T;
        }
    }
}
