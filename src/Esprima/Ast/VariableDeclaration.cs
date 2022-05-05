using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class VariableDeclaration : Declaration
    {
        internal readonly NodeList<VariableDeclarator> _declarations;

        public VariableDeclaration(
            in NodeList<VariableDeclarator> declarations,
            VariableDeclarationKind kind)
            : base(Nodes.VariableDeclaration)
        {
            _declarations = declarations;
            Kind = kind;
        }

        public ReadOnlySpan<VariableDeclarator> Declarations { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _declarations.AsSpan(); }
        public VariableDeclarationKind Kind { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(_declarations);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitVariableDeclaration(this);
        }

        public VariableDeclaration UpdateWith(in NodeList<VariableDeclarator> declarations)
        {
            if (NodeList.AreSame(declarations, _declarations))
            {
                return this;
            }

            return new VariableDeclaration(declarations, Kind);
        }
    }
}
