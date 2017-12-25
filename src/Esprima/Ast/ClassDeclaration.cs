namespace Esprima.Ast
{
    public class ClassDeclaration : Node,
        Declaration
    {
        public Identifier Id { get; }
        public PropertyKey SuperClass { get; }
        public ClassBody Body { get; }

        public ClassDeclaration(Identifier id, PropertyKey superClass, ClassBody body)
        {
            Type = Nodes.ClassDeclaration;
            Id = id;
            SuperClass = superClass;
            Body = body;
        }
    }
}
