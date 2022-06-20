using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportNamedDeclaration : ExportDeclaration
    {
        private readonly NodeList<ExportSpecifier> _specifiers;
        private readonly NodeList<ImportAttribute> _assertions;

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
        public ref readonly NodeList<ExportSpecifier> Specifiers { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _specifiers; }
        public Literal? Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ref readonly NodeList<ImportAttribute> Assertions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _assertions; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        private IEnumerable<Node?> CreateChildNodes()
        {
            yield return Declaration;

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

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitExportNamedDeclaration(this, context);
        }

        public ExportNamedDeclaration UpdateWith(StatementListItem? declaration, in NodeList<ExportSpecifier> specifiers, Literal? source, in NodeList<ImportAttribute> assertions)
        {
            if (declaration == Declaration && NodeList.AreSame(specifiers, Specifiers) && source == Source && NodeList.AreSame(assertions, Assertions))
            {
                return this;
            }

            return new ExportNamedDeclaration(declaration, specifiers, source, assertions);
        }
    }
}
