using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class UpdateExpression : UnaryExpression
    {
        public UpdateExpression(string? op, Expression arg, bool prefix) : base(Nodes.UpdateExpression, op, arg)
        {
            Prefix = prefix;
        }
        
        internal UpdateExpression(UnaryOperator op, Expression arg, bool prefix) : base(Nodes.UpdateExpression, op, arg)
        {
            Prefix = prefix;
        }

        protected internal override Node Accept(AstVisitor visitor)
        {
            return visitor.VisitUpdateExpression(this);
        }
    }
}
