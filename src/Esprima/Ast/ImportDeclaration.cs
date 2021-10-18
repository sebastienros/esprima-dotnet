﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportDeclaration : Declaration
    {
        private readonly NodeList<ImportDeclarationSpecifier> _specifiers;

        public readonly Literal Source;

        public ImportDeclaration(
            in NodeList<ImportDeclarationSpecifier> specifiers,
            Literal source)
            : base(Nodes.ImportDeclaration)
        {
            _specifiers = specifiers;
            Source = source;
        }

        public ref readonly NodeList<ImportDeclarationSpecifier> Specifiers => ref _specifiers;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_specifiers, Source);

        protected internal override void Accept(AstVisitor visitor)
        {
            visitor.VisitImportDeclaration(this);
        }
    }
}
