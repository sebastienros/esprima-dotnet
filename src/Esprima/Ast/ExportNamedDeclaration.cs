using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ExportNamedDeclaration : Node, ExportDeclaration
    {
        public StatementListItem Declaration { get; }
        public List<ExportSpecifier> Specifiers { get; }
        public Literal Source { get; }

        public ExportNamedDeclaration(StatementListItem declaration, List<ExportSpecifier> specifiers, Literal source)
        {
            Type = Nodes.ExportNamedDeclaration;
            Declaration = declaration;
            Specifiers = specifiers;
            Source = source;
        }
    }
}