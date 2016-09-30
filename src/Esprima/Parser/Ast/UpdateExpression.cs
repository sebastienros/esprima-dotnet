namespace Esprima.Ast
{
    public class UpdateExpression : UnaryExpression
    {
        public UpdateExpression(string op, Expression arg, bool prefix) : base(op, arg)
        {
            Type = Nodes.UpdateExpression;
            Prefix = prefix;
        }
    }
}