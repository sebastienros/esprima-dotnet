using System.Collections.Generic;
using System.Linq;

namespace Esprima.Ast
{
    public class Identifier : Node,
        BindingIdentifier,
        Expression
    {
        public readonly string Name;

        public Identifier(string name) :
            base(Nodes.Identifier)
        {
            Name = name;
        }

        public override IEnumerable<INode> ChildNodes => ZeroChildNodes;
    }
}