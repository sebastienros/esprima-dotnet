using Esprima.Utils;

namespace Esprima.Ast
{
    public class AttributeVariableDefinition : Expression
    {
        public Expression VarExpression { get; set; }

        public AttributeVariableDefinition(Nodes node, Location location)
            : base(node)
        {
            Location = location;
        }

        public override NodeCollection ChildNodes => new NodeCollection(VarExpression);

        protected internal override void Accept(AstVisitor visitor)
        {

        }
    }
}
