using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportDeclaration : Declaration
    {
        internal readonly NodeList<ImportDeclarationSpecifier> _specifiers;
        internal readonly NodeList<ImportAttribute> _assertions;

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

        public ReadOnlySpan<ImportDeclarationSpecifier> Specifiers { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _specifiers.AsSpan(); }
        public Literal Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ReadOnlySpan<ImportAttribute> Assertions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _assertions.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        private IEnumerable<Node?> CreateChildNodes()
        {
            foreach (var node in _specifiers)
            {
                yield return node;
            }

            yield return Source;

            foreach (var node in _assertions)
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
            if (NodeList.AreSame(specifiers, _specifiers) && source == Source && NodeList.AreSame(assertions, _assertions))
            {
                return this;
            }

            return new ImportDeclaration(specifiers, source, assertions);
        }
    }
}
