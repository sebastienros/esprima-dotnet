using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportDefaultDeclaration : ExportDeclaration
    {
        public readonly StatementListItem Declaration; //: BindingIdentifier | BindingPattern | ClassDeclaration | Expression | FunctionDeclaration;

        public ExportDefaultDeclaration(StatementListItem declaration) : base(Nodes.ExportDefaultDeclaration)
        {
            Declaration = declaration;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Declaration);

        public override void Accept(AstVisitor visitor) => visitor.VisitExportDefaultDeclaration(this);
    }
}