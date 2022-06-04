using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ExportNamedDeclaration : ExportDeclaration
    {
        private readonly NodeList<ExportSpecifier> _specifiers;

        public readonly StatementListItem? Declaration;
        public readonly Literal? Source;
        public readonly NodeList<ImportAttribute> Assertions;
        
        public ExportNamedDeclaration(
            StatementListItem? declaration,
            in NodeList<ExportSpecifier> specifiers,
            Literal? source,
            in NodeList<ImportAttribute> assertions)
            : base(Nodes.ExportNamedDeclaration)
        {
            Declaration = declaration;
            _specifiers = specifiers;
            Source = source;
            Assertions = assertions;
        }

        public ref readonly NodeList<ExportSpecifier> Specifiers => ref _specifiers;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(NodeList.Create(CreateChildNodes()));

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitExportNamedDeclaration(this);
        }

        public ExportNamedDeclaration UpdateWith(StatementListItem? declaration, in NodeList<ExportSpecifier> specifiers, Literal? source, in NodeList<ImportAttribute> assertions)
        {
            if (declaration == Declaration && NodeList.AreSame(specifiers, Specifiers) && source == Source && NodeList.AreSame(assertions, Assertions))
            {
                return this;
            }

            return new ExportNamedDeclaration(declaration, specifiers, source, assertions);
        }
                
        private IEnumerable<Node> CreateChildNodes()
        {
            if(Declaration is not null){
                yield return Declaration;
            }
            
            foreach (var node in _specifiers)
            {
                yield return node;
            }

            if(Source is not null){
                yield return Source;
            }
            
            foreach (var node in Assertions)
            {
                yield return node;
            }
        }
    }
}
