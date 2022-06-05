using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class MethodDefinition : ClassProperty
    {
        public readonly bool Static;
        public readonly NodeList<Decorator> Decorators;

        public MethodDefinition(
            Expression key,
            bool computed,
            FunctionExpression value,
            PropertyKind kind,
            bool isStatic,
            in NodeList<Decorator> decorators)
            : base(Nodes.MethodDefinition)
        {
            Static = isStatic;
            Key = key;
            Computed = computed;
            Value = value;
            Kind = kind;
            Decorators = decorators;
        }

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitMethodDefinition(this);
        }

        public MethodDefinition UpdateWith(Expression key, FunctionExpression value, in NodeList<Decorator> decorators)
        {
            if (key == Key && value == Value && NodeList.AreSame(decorators, Decorators))
            {
                return this;
            }

            return new MethodDefinition(key, Computed, value, Kind, Static, decorators);
        }
    }
}
