using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class Super : Expression
    {
        public Super() : base(Nodes.Super)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitSuper(this, context);
        }
    }
}
