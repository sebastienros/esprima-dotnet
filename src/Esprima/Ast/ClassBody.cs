using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ClassBody : Node
    {
        public readonly List<ClassProperty> Body;

        public ClassBody(List<ClassProperty> body)
        {
            Type = Nodes.ClassBody;
            Body = body;
        }
    }
}
