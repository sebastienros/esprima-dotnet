using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportAllDeclaration : ExportDeclaration
    {
        public readonly Literal Source;

        /// <summary>
        /// Identifier | StringLiteral
        /// </summary>
        public readonly Expression? Exported;
        
        public readonly NodeList<ImportAttribute> Assertions;

        public ExportAllDeclaration(Literal source) : this(source, null, new NodeList<ImportAttribute>())
        {
        }

        public ExportAllDeclaration(Literal source, Expression? exported, in NodeList<ImportAttribute> assertions) : base(Nodes.ExportAllDeclaration)
        {
            Source = source;
            Exported = exported;
            Assertions = assertions;
        }

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitExportAllDeclaration(this);
        }

        public ExportAllDeclaration UpdateWith(Expression? exported, Literal source, in NodeList<ImportAttribute> assertions)
        {
            if (exported == Exported && source == Source && NodeList.AreSame(assertions, Assertions))
            {
                return this;
            }

            return new ExportAllDeclaration(source, exported, assertions);
        }
        
        private IEnumerable<Node> CreateChildNodes()
        {
            yield return Source;
            
            if (Exported is not null)
            {
                yield return Exported;
            }
            
            foreach (var assertion in Assertions)
            {
                yield return assertion;
            }
        }
    }
}
