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

        public override NodeCollection ChildNodes => new(Declaration);

        protected internal override Node Accept(AstVisitor visitor)
        {
            return visitor.VisitExportDefaultDeclaration(this);
        }
    }
}
