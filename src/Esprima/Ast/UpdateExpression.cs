namespace Esprima.Ast
{
    public class UpdateExpression : UnaryExpression
    {
        public UpdateExpression(string op, Expression arg, bool prefix) :
            base(Nodes.UpdateExpression, op, arg)
        {
            Prefix = prefix;
        }
    }
}