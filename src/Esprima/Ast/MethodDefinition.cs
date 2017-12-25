namespace Esprima.Ast
{
    public class MethodDefinition : ClassProperty
    {
        public bool Static { get; }

        public MethodDefinition(PropertyKey key, bool computed, FunctionExpression value, PropertyKind kind, bool isStatic)
        {
            Type = Nodes.MethodDefinition;
            Static = isStatic;
            Key = key;
            Computed = computed;
            Value = value;
            Kind = kind;
        }
    }
}
