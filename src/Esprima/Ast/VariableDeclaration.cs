using System.Collections.Generic;

namespace Esprima.Ast
{
    public class VariableDeclaration : Statement,
        Declaration
    {
        public IEnumerable<VariableDeclarator> Declarations;
        public string Kind;

        public VariableDeclaration(IEnumerable<VariableDeclarator> declarations, string kind)
        {
            Type = Nodes.VariableDeclaration;
            Declarations = declarations;
            Kind = kind;
        }

    }
}