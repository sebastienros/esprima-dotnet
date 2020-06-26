namespace Esprima.Ast
{
    public sealed class UpdateExpression : UnaryExpression
    {
        public UpdateExpression(string? op, Expression arg, bool prefix) : base(Nodes.UpdateExpression, op, arg)
        {
            Prefix = prefix;
        }
    }
}