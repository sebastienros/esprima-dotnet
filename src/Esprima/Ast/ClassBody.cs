using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ClassBody : Node
    {
        public readonly NodeList<ClassProperty> Body;

        public ClassBody(NodeList<ClassProperty> body) :
            base(Nodes.ClassBody)
        {
            Body = body;
        }

        public override IEnumerable<INode> ChildNodes =>
            ChildNodeYielder.Yield(Body);
    }
}
