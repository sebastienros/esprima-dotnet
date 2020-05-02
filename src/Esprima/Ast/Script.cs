namespace Esprima.Ast
{
    public sealed class Script : Program
    {
        private readonly NodeList<Statement> _body;

        public Script(
            in NodeList<Statement> body,
            bool strict)
            : base(Nodes.Program)
        {
            _body = body;
            Strict = strict;
        }

        public override SourceType SourceType => SourceType.Script;
        public bool Strict { get; }

        public override ref readonly NodeList<Statement> Body => ref _body;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Body);
    }
}