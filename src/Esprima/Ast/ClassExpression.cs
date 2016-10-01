namespace Esprima.Ast
{
    public class ClassExpression : Node,
        Expression
    {
        public Identifier Id;
        public PropertyKey SuperClass;
        public ClassBody Body;

        public ClassExpression(Identifier id, PropertyKey superClass, ClassBody body)
            {
                Type = Nodes.ClassExpression;
                Id = id;
                SuperClass = superClass;
                Body = body;
            }
    }
}
