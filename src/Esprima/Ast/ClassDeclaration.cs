namespace Esprima.Ast
{
    public class ClassDeclaration : Node,
        Declaration
    {
        public Identifier Id;
        public PropertyKey SuperClass;
        public ClassBody Body;
        public ClassDeclaration(Identifier id, PropertyKey superClass, ClassBody body)
            {
                Type = Nodes.ClassDeclaration;
                Id = id;
                SuperClass = superClass;
                Body = body;
            }
    }
}
