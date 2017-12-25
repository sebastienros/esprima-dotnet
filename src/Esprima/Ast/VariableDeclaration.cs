using System.Collections.Generic;

namespace Esprima.Ast
{
    public class VariableDeclaration : Statement,
        Declaration
    {
        public List<VariableDeclarator> Declarations { get; }
        public string Kind { get; }

        public VariableDeclaration(List<VariableDeclarator> declarations, string kind)
        {
            Type = Nodes.VariableDeclaration;
            Declarations = declarations;
            Kind = kind;
        }

    }
}