using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ClassBody : Node
    {
        public readonly List<ClassProperty> Body;

        public ClassBody(List<ClassProperty> body) :
            base(Nodes.ClassBody)
        {
            Body = body;
        }
    }
}
