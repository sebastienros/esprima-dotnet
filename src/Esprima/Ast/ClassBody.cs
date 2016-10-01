using System.Collections.Generic;

namespace Esprima.Ast
{
    public class ClassBody : Node
    {
        public IEnumerable<ClassProperty> Body;
        public ClassBody(IEnumerable<ClassProperty> body)
            {
                Type = Nodes.ClassBody;
                Body = body;
            }
        }
}
