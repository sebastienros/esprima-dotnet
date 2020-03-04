using System.Collections.Generic;

namespace Esprima.Ast
{
    public class Module : Statement, Program
    {
        private readonly NodeList<IStatementListItem> _body;
        public SourceType SourceType => SourceType.Module;

        public Module(
            in NodeList<IStatementListItem> body,
            HoistingScope hoistingScope) :
            base(Nodes.Program)
        {
            _body = body;
            HoistingScope = hoistingScope;
        }

        public HoistingScope HoistingScope { get; }

        public ref readonly NodeList<IStatementListItem> Body => ref _body;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Body);
    }
}