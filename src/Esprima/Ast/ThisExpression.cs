using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ThisExpression : Expression
    {
        public ThisExpression() : base(Nodes.ThisExpression)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        public override void Accept(AstVisitor visitor) => visitor.VisitThisExpression(this);
    }
}