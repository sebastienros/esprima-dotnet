using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportDeclaration : Declaration
    {
        private readonly NodeList<ImportDeclarationSpecifier> _specifiers;

        public readonly Literal Source;

        public readonly NodeList<ImportAttribute> Assertions;
        public ImportDeclaration(
            in NodeList<ImportDeclarationSpecifier> specifiers,
            Literal source,
            in NodeList<ImportAttribute> assertions)
            : base(Nodes.ImportDeclaration)
        {
            _specifiers = specifiers;
            Source = source;
            Assertions = assertions;
        }

        public ref readonly NodeList<ImportDeclarationSpecifier> Specifiers => ref _specifiers;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitImportDeclaration(this);
        }

        public ImportDeclaration UpdateWith(in NodeList<ImportDeclarationSpecifier> specifiers, Literal source, in NodeList<ImportAttribute> assertions)
        {
            if (NodeList.AreSame(specifiers, Specifiers) && source == Source && NodeList.AreSame(assertions, Assertions))
            {
                return this;
            }

            return new ImportDeclaration(specifiers, source, assertions).SetAdditionalInfo(this);
        }

        private IEnumerable<Node> CreateChildNodes()
        {
            foreach (var node in _specifiers)
            {
                yield return node;
            }

            yield return Source;

            foreach (var node in Assertions)
            {
                yield return node;
            }
        }
    }
}
