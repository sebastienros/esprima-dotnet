using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportDefaultDeclaration : ExportDeclaration
    {
        public ExportDefaultDeclaration(StatementListItem declaration) : base(Nodes.ExportDefaultDeclaration)
        {
            Declaration = declaration;
        }

        /// <remarks>
        /// BindingIdentifier | <see cref="BindingPattern" /> | <see cref="ClassDeclaration" /> | <see cref="Expression" /> | <see cref="FunctionDeclaration" />
        /// </remarks>
        public StatementListItem Declaration { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        public override NodeCollection ChildNodes => new(Declaration);

        protected internal override object? Accept(AstVisitor visitor, object? context)
        {
            return visitor.VisitExportDefaultDeclaration(this, context);
        }

        public ExportDefaultDeclaration UpdateWith(StatementListItem declaration)
        {
            if (declaration == Declaration)
            {
                return this;
            }

            return new ExportDefaultDeclaration(declaration);
        }
    }
}
