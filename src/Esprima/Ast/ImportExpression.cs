using Esprima.Utils;

namespace Esprima.Ast
{
    public class ImportExpression : Expression
    {
        public ImportDeclaration Declaration { get; set; }

        public ImportExpression(Location location)
            : base(Nodes.ImportDeclaration)
        {
            Location = location;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Declaration);

        protected internal override void Accept(AstVisitor visitor)
        {

        }
    }
}
