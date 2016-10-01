using System.Collections.Generic;

namespace Esprima.Ast
{
    public class VariableDeclaration : Node,
        Declaration,
        Statement
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