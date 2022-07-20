using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportSpecifier : Node
    {
        public ExportSpecifier(Expression local, Expression exported) : base(Nodes.ExportSpecifier)
        {
            Local = local;
            Exported = exported;
        }

        /// <remarks>
        /// <see cref="Identifier"/> | <see cref="Literal"/> (string)
        /// </remarks>
        public Expression Local { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        /// <remarks>
        /// <see cref="Identifier"/> | <see cref="Literal"/> (string)
        /// </remarks>
        public Expression Exported { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }

        internal override Node? NextChildNode(ref ChildNodes.Enumerator enumerator) => enumerator.MoveNextExportSpecifier(Local, Exported);

        protected internal override object? Accept(AstVisitor visitor) => visitor.VisitExportSpecifier(this);

        public ExportSpecifier UpdateWith(Expression local, Expression exported)
        {
            if (local == Local && exported == Exported)
            {
                return this;
            }

            return new ExportSpecifier(local, exported);
        }
    }
}
