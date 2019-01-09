namespace Esprima.Ast
{
    public class ClassDeclaration : Node,
        Declaration
    {
        public readonly Identifier Id;
        public readonly Expression SuperClass; // Identifier || CallExpression
        public readonly ClassBody Body;

        public ClassDeclaration(Identifier id, Expression superClass, ClassBody body) :
            base(Nodes.ClassDeclaration)
        {
            Id = id;
            SuperClass = superClass;
            Body = body;
        }
    }
}
