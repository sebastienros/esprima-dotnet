namespace Esprima.Ast
{
    public class ClassExpression : Node, Expression
    {
        public Identifier Id { get; }
        public PropertyKey SuperClass { get; }
        public ClassBody Body { get; }

        public ClassExpression(Identifier id, PropertyKey superClass, ClassBody body)
        {
            Type = Nodes.ClassExpression;
            Id = id;
            SuperClass = superClass;
            Body = body;
        }
    }
}