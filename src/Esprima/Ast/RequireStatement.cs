using Esprima.Utils;

namespace Esprima.Ast
{
    public class RequireStatement : Statement
    {
        public Expression Path { get; set; }

        public RequireStatement() : base(Nodes.RequireStatement)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {
            
        }
    }
}
