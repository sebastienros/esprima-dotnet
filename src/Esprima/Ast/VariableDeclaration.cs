using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class VariableDeclaration : Declaration
    {
        private readonly NodeList<VariableDeclarator> _declarations;
        public readonly VariableDeclarationKind Kind;

        public VariableDeclaration(
            in NodeList<VariableDeclarator> declarations,
            VariableDeclarationKind kind)
            : base(Nodes.VariableDeclaration)
        {
            _declarations = declarations;
            Kind = kind;
        }

        public ref readonly NodeList<VariableDeclarator> Declarations => ref _declarations;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_declarations);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitVariableDeclaration(this);
        }

        public VariableDeclaration UpdateWith(in NodeList<VariableDeclarator> declarations)
        {
            if (NodeList.AreSame(declarations, Declarations))
            {
                return this;
            }

            return new VariableDeclaration(declarations, Kind);
        }
    }
}
