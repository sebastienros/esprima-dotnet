using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ImportDeclaration : Declaration
    {
        private readonly NodeList<ImportDeclarationSpecifier> _specifiers;

        public readonly Literal Source;

        public ImportDeclaration(
            in NodeList<ImportDeclarationSpecifier> specifiers,
            Literal source) :
            base(Nodes.ImportDeclaration)
        {
            _specifiers = specifiers;
            Source = source;
        }

        public ref readonly NodeList<ImportDeclarationSpecifier> Specifiers => ref _specifiers;

        public override IEnumerable<Node> ChildNodes => ChildNodeYielder.Yield(_specifiers, Source);
    }
}
