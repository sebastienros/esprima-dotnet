﻿using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportNamedDeclaration : ExportDeclaration
    {
        private readonly NodeList<ExportSpecifier> _specifiers;

        public readonly StatementListItem? Declaration;
        public readonly Literal? Source;

        public ExportNamedDeclaration(
            StatementListItem? declaration,
            in NodeList<ExportSpecifier> specifiers,
            Literal? source)
            : base(Nodes.ExportNamedDeclaration)
        {
            Declaration = declaration;
            _specifiers = specifiers;
            Source = source;
        }

        public ref readonly NodeList<ExportSpecifier> Specifiers => ref _specifiers;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Declaration, _specifiers, Source);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitExportNamedDeclaration(this);
        }

        public ExportNamedDeclaration UpdateWith(StatementListItem? declaration, in NodeList<ExportSpecifier> specifiers, Literal? source)
        {
            if (declaration == Declaration && NodeList.AreSame(specifiers, Specifiers) && source == Source)
            {
                return this;
            }

            return new ExportNamedDeclaration(declaration, specifiers, source);
        }
    }
}
