using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ImportDeclaration : Node, Declaration
    {
        public readonly List<ImportDeclarationSpecifier> Specifiers;
        public readonly Literal Source;

        public ImportDeclaration(List<ImportDeclarationSpecifier> specifiers, Literal source)
        {
            Type = Nodes.ImportDeclaration;
            Specifiers = specifiers;
            Source = source;
        }
    }
}