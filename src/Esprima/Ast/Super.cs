using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class Super : Expression
    {
        public Super() : base(Nodes.Super)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override Node Accept(AstVisitor visitor)
        {
            return visitor.VisitSuper(this);
        }
    }
}
