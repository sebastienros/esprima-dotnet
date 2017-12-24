namespace Esprima.Ast
{
    public class ClassDeclaration : Node,
        Declaration
    {
        public readonly Identifier Id;
        public readonly PropertyKey SuperClass;
        public readonly ClassBody Body;

        public ClassDeclaration(Identifier id, PropertyKey superClass, ClassBody body)
        {
            Type = Nodes.ClassDeclaration;
            Id = id;
            SuperClass = superClass;
            Body = body;
        }
    }
}
