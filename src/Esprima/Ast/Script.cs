using System.Collections.Generic;

namespace Esprima.Ast
{
    public class Script : Statement, Program
    {
        private readonly NodeList<IStatementListItem> _body;

        public Script(
            in NodeList<IStatementListItem> body,
            bool strict,
            HoistingScope hoistingScope)
            : base(Nodes.Program)
        {
            _body = body;
            Strict = strict;
            HoistingScope = hoistingScope;
        }

        public SourceType SourceType => SourceType.Script;
        public bool Strict { get; }

        public HoistingScope HoistingScope { get; }
        public ref readonly NodeList<IStatementListItem> Body => ref _body;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Body);
    }
}