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

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitCatchClause(this);
        }

        public CatchClause UpdateWith(Expression? param, BlockStatement body)
        {
            if (param == Param && body == Body)
            {
                return this;
            }

            return new CatchClause(param, body).SetAdditionalInfo(this);
        }
    }
}
