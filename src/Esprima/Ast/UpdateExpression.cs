using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class UpdateExpression : UnaryExpression
    {
        public UpdateExpression(string? op, Expression arg, bool prefix) : base(Nodes.UpdateExpression, op, arg)
        {
            Prefix = prefix;
        }

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitUpdateExpression(this) as T;
        }
    }
}
