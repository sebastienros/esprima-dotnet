using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class SelfExpression : Expression
    {
        public SelfExpression() : base(Nodes.SelfExpression)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitSelfExpression(this);
        }
    }
}
