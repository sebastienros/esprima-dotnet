using System.Collections.Generic;

namespace Esprima.Ast
{
    public class VariableDeclaration : Statement, IDeclaration
    {
        public readonly VariableDeclarationKind Kind;
        private readonly NodeList<VariableDeclarator> _declarations;

        public VariableDeclaration(
            in NodeList<VariableDeclarator> declarations,
            VariableDeclarationKind kind) :
            base(Nodes.VariableDeclaration)
        {
            _declarations = declarations;
            Kind = kind;
        }

        public ref readonly NodeList<VariableDeclarator> Declarations => ref _declarations;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(_declarations);
    }
}