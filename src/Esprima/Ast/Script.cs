namespace Esprima.Ast
{
    public sealed class Script : Program
    {
        private readonly NodeList<Statement> _body;

        public Script(
            in NodeList<Statement> body,
            bool strict,
            HoistingScope hoistingScope)
            : base(Nodes.Program)
        {
            _body = body;
            Strict = strict;
            HoistingScope = hoistingScope;
        }

        public override SourceType SourceType => SourceType.Script;
        public bool Strict { get; }

        public HoistingScope HoistingScope { get; }
        public override ref readonly NodeList<Statement> Body => ref _body;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Body);
    }
}