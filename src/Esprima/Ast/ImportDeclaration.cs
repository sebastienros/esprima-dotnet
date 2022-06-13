using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportDeclaration : Declaration
    {
        private readonly NodeList<ImportDeclarationSpecifier> _specifiers;
        private readonly NodeList<ImportAttribute> _assertions;

        public ImportDeclaration(
            in NodeList<ImportDeclarationSpecifier> specifiers,
            Literal source,
            in NodeList<ImportAttribute> assertions)
            : base(Nodes.ImportDeclaration)
        {
            _specifiers = specifiers;
            Source = source;
            _assertions = assertions;
        }

        public ref readonly NodeList<ImportDeclarationSpecifier> Specifiers { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _specifiers; }
        public Literal Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ref readonly NodeList<ImportAttribute> Assertions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _assertions; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        private IEnumerable<Node?> CreateChildNodes()
        {
            foreach (var node in Specifiers)
            {
                yield return node;
            }

            yield return Source;

            foreach (var node in Assertions)
            {
                yield return node;
            }
        }

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

            return new ImportDeclaration(specifiers, source, assertions);
        }
    }
}
