using System.Collections.Generic;

namespace Esprima.Ast
{
    public class Program : Statement
    {
        private readonly NodeList<IStatementListItem> _body;

        public readonly SourceType SourceType;

        public HoistingScope HoistingScope { get; }
        public bool Strict { get; }

        public Program(
            in NodeList<IStatementListItem> body,
            SourceType sourceType,
            HoistingScope hoistingScope,
            bool strict) :
            base(Nodes.Program)
        {
            _body = body;
            SourceType = sourceType;
            HoistingScope = hoistingScope;
            Strict = strict;
        }

        public ref readonly NodeList<IStatementListItem> Body => ref _body;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Body);
    }
}