namespace Esprima.Ast
{
    public class ClassExpression : Node, Expression
    {
        public readonly Identifier Id;
        public readonly PropertyKey SuperClass;
        public readonly ClassBody Body;

        public ClassExpression(Identifier id, PropertyKey superClass, ClassBody body)
        {
            Type = Nodes.ClassExpression;
            Id = id;
            SuperClass = superClass;
            Body = body;
        }
    }
}