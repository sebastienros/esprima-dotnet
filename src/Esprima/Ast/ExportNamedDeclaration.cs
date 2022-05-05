using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportNamedDeclaration : ExportDeclaration
    {
        internal readonly NodeList<ExportSpecifier> _specifiers;
        internal readonly NodeList<ImportAttribute> _assertions;

        public ExportNamedDeclaration(
            StatementListItem? declaration,
            in NodeList<ExportSpecifier> specifiers,
            Literal? source,
            in NodeList<ImportAttribute> assertions)
            : base(Nodes.ExportNamedDeclaration)
        {
            Declaration = declaration;
            _specifiers = specifiers;
            Source = source;
            _assertions = assertions;
        }

        public StatementListItem? Declaration { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ReadOnlySpan<ExportSpecifier> Specifiers { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _specifiers.AsSpan(); }
        public Literal? Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ReadOnlySpan<ImportAttribute> Assertions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _assertions.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        private IEnumerable<Node?> CreateChildNodes()
        {
            yield return Declaration;

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
            return visitor.VisitExportNamedDeclaration(this);
        }

        public ExportNamedDeclaration UpdateWith(StatementListItem? declaration, in NodeList<ExportSpecifier> specifiers, Literal? source, in NodeList<ImportAttribute> assertions)
        {
            if (declaration == Declaration && NodeList.AreSame(specifiers, _specifiers) && source == Source && NodeList.AreSame(assertions, _assertions))
            {
                return this;
            }

            return new ExportNamedDeclaration(declaration, specifiers, source, assertions);
        }
    }
}
