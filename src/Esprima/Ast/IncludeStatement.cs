using Esprima.Utils;

namespace Esprima.Ast
{
    public class IncludeStatement : Statement
    {
        public string Path { get; set; }

        public IncludeStatement() : base(Nodes.IncludeStatement)
        {
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {
            
        }
    }
}
