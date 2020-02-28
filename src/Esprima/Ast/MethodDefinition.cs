namespace Esprima.Ast
{
    public class MethodDefinition : ClassProperty
    {
        public readonly bool Static;

        public MethodDefinition(Expression key, bool computed, IFunctionExpression value, PropertyKind kind, bool isStatic) :
            base(Nodes.MethodDefinition)
        {
            Static = isStatic;
            Key = key;
            Computed = computed;
            Value = value;
            Kind = kind;
        }
    }
}
