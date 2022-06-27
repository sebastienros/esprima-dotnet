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

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator)
        {
            switch (enumerator._propertyIndex)
            {
                case 0:
                    enumerator._propertyIndex++;

                    if (Declaration is null)
                    {
                        enumerator._listIndex = 0;
                        goto case 1;
                    }

                    return Declaration;
                case 1:
                    if (enumerator._listIndex >= Specifiers.Count)
                    {
                        enumerator._propertyIndex++;
                        goto case 2;
                    }

                    return Specifiers[enumerator._listIndex++];
                case 2:
                    enumerator._propertyIndex++;

                    if (Source is null)
                    {
                        enumerator._listIndex = 0;
                        goto case 3;
                    }

                    return Source;
                case 3:
                    if (enumerator._listIndex >= Assertions.Count)
                    {
                        enumerator._propertyIndex++;
                        goto default;
                    }

                    return Assertions[enumerator._listIndex++];
                default:
                    return null;
            }
        }

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitExportNamedDeclaration(this);
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
