using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportDeclaration : Declaration
    {
        private readonly NodeList<ImportDeclarationSpecifier> _specifiers;

        public ImportDeclaration(
            in NodeList<ImportDeclarationSpecifier> namespacePath, Identifier target)
            : base(Nodes.ImportDeclaration)
        {
            _specifiers = namespacePath;
            Target = target;
        }

        public Identifier Target { get; set; }

        public ref readonly NodeList<ImportDeclarationSpecifier> Specifiers => ref _specifiers;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_specifiers, Target);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitImportDeclaration(this);
        }
    }
}
