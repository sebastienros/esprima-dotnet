using System.Collections.Generic;

namespace Esprima.Ast
{
    public class SwitchStatement : Statement
    {
        private readonly NodeList<SwitchCase> _cases;

        public readonly Expression Discriminant;

        public SwitchStatement(Expression discriminant, in NodeList<SwitchCase> cases) :
            base(Nodes.SwitchStatement)
        {
            Discriminant = discriminant;
            _cases = cases;
        }

        public ref readonly NodeList<SwitchCase> Cases => ref _cases;

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Discriminant, _cases);
    }
}