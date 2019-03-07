using System.Collections.Generic;

namespace Esprima.Ast
{
    public class VariableDeclaration : Statement,
        IDeclaration
    {
        public readonly NodeList<VariableDeclarator> Declarations;
        public readonly VariableDeclarationKind Kind;

        public VariableDeclaration(NodeList<VariableDeclarator> declarations, VariableDeclarationKind kind) :
            base(Nodes.VariableDeclaration)
        {
            Declarations = declarations;
            Kind = kind;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Declarations);
    }
}