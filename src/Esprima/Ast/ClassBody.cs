using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ClassBody : Node
    {
        public List<ClassProperty> Body { get; }

        public ClassBody(List<ClassProperty> body)
        {
            Type = Nodes.ClassBody;
            Body = body;
        }
    }
}
