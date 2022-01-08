using Esprima.Utils;

namespace Esprima.Ast
{
    public class StaticVariableDefinition : Expression
    {
        public Expression VarExpression { get; set; }

        public StaticVariableDefinition(Nodes node, Location location)
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
