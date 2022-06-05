using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class PropertyDefinition : ClassProperty
    {
        public readonly bool Static;
        public readonly NodeList<Decorator> Decorators;

        public PropertyDefinition(
            Expression key,
            bool computed,
            Expression value,
            bool isStatic,
            in NodeList<Decorator> decorators)
            : base(Nodes.PropertyDefinition)
        {
            Static = isStatic;
            Key = key;
            Computed = computed;
            Value = value;
            Decorators = decorators;
        }

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitPropertyDefinition(this);
        }

        public PropertyDefinition UpdateWith(Expression key, Expression? value, in NodeList<Decorator> decorators)
        {
            if (key == Key && value == Value && NodeList.AreSame(decorators, Decorators))
            {
                return this;
            }

            return new PropertyDefinition(key, Computed, value!, Static, decorators);
        }
    }
}
