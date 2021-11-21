using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ThisExpression : Expression
    {
        public ThisExpression() : base(Nodes.ThisExpression)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override Node? Accept(AstVisitor visitor)
        {
            return visitor.VisitThisExpression(this);
        }
    }
}
