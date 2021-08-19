using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class Import : Expression
    {
        public Import() : base(Nodes.Import)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor) => visitor.VisitImport(this);
    }
}
