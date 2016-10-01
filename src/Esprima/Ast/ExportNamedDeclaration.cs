using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ExportNamedDeclaration : Node,
        ExportDeclaration
    {
        public StatementListItem Declaration;
        public List<ExportSpecifier> Specifiers;
        public Literal Source;
        public ExportNamedDeclaration(StatementListItem declaration, List<ExportSpecifier> specifiers, Literal source)
        {
            Type = Nodes.ExportNamedDeclaration;
            Declaration = declaration;
            Specifiers = specifiers;
            Source = source;
        }
    }
}
