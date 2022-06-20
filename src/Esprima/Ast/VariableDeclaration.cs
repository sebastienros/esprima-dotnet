using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class VariableDeclaration : Declaration
    {
        private readonly NodeList<VariableDeclarator> _declarations;

        public VariableDeclaration(
            in NodeList<VariableDeclarator> declarations,
            VariableDeclarationKind kind)
            : base(Nodes.VariableDeclaration)
        {
            _declarations = declarations;
            Kind = kind;
        }

        public ref readonly NodeList<VariableDeclarator> Declarations { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ref _declarations; }
        public VariableDeclarationKind Kind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Declarations);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitVariableDeclaration(this, context);
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
