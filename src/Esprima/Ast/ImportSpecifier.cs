using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ImportSpecifier : ImportDeclarationSpecifier
    {
        public ImportSpecifier(Identifier local, Expression imported) : base(local, Nodes.ImportSpecifier)
        {
            Imported = imported;
        }

        /// <remarks>
        /// <see cref="Identifier" /> | StringLiteral (<see cref="Literal" />)
        /// </remarks>
        public Expression Imported { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNext(Imported, Local);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitImportSpecifier(this);

        public ImportSpecifier UpdateWith(Expression imported, Identifier local)
        {
            if (imported == Imported && local == Local)
            {
                return this;
            }

            return new ImportSpecifier(local, imported);
        }
    }
}
