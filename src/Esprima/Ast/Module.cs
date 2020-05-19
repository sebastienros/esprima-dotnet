namespace Esprima.Ast
{
    public sealed class Module : Program
    {
        private readonly NodeList<Statement> _body;
        public override SourceType SourceType => SourceType.Module;

        public Module(
            in NodeList<Statement> body,
            HoistingScope hoistingScope) 
            : base(Nodes.Program)
        {
            _body = body;
            HoistingScope = hoistingScope;
        }

        public HoistingScope HoistingScope { get; }

        public override ref readonly NodeList<Statement> Body => ref _body;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Body);
    }
}