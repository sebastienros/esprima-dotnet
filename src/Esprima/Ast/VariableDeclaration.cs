using System.Collections.Generic;

namespace Esprima.Ast
{
    public class VariableDeclaration : Statement,
        Declaration
    {
        public readonly List<VariableDeclarator> Declarations;
        public readonly string Kind;

        public VariableDeclaration(List<VariableDeclarator> declarations, string kind)
        {
            Type = Nodes.VariableDeclaration;
            Declarations = declarations;
            Kind = kind;
        }

    }
}