using System.Collections.Generic;

namespace Esprima.Ast
{
    public class Program : Statement
    {
        public readonly List<IStatementListItem> Body;
        public readonly SourceType SourceType;

        public HoistingScope HoistingScope { get; }
        public bool Strict { get; }

        public Program(List<IStatementListItem> body, SourceType sourceType, HoistingScope hoistingScope, bool strict) :
            base(Nodes.Program)
        {
            Body = body;
            SourceType = sourceType;
            HoistingScope = hoistingScope;
            Strict = strict;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Body);
    }
}