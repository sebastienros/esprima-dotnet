using System.Runtime.CompilerServices;
using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportAllDeclaration : ExportDeclaration
    {
        internal readonly NodeList<ImportAttribute> _assertions;

        public ExportAllDeclaration(Literal source) : this(source, null, new NodeList<ImportAttribute>())
        {
        }

        public ExportAllDeclaration(Literal source, Expression? exported, in NodeList<ImportAttribute> assertions) : base(Nodes.ExportAllDeclaration)
        {
            Source = source;
            Exported = exported;
            _assertions = assertions;
        }

        public Literal Source { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        /// <remarks>
        /// <see cref="Identifier" /> | StringLiteral (<see cref="Literal" />)
        /// </remarks>
        public Expression? Exported { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
        public ReadOnlySpan<ImportAttribute> Assertions { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _assertions.AsSpan(); }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        private IEnumerable<Node?> CreateChildNodes()
        {
            yield return Exported;
            yield return Source;

            foreach (var assertion in _assertions)
            {
                yield return assertion;
            }
        }

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitExportAllDeclaration(this);
        }

        public ExportAllDeclaration UpdateWith(Expression? exported, Literal source, in NodeList<ImportAttribute> assertions)
        {
            if (exported == Exported && source == Source && NodeList.AreSame(assertions, _assertions))
            {
                return this;
            }

            return new ExportAllDeclaration(source, exported, assertions);
        }
    }
}
