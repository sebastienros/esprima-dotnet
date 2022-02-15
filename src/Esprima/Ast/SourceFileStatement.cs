using Esprima.Utils;

namespace Esprima.Ast
{
    public class SourceFileStatement : Statement
    {
        public string Path { get; set; }

        public SourceFileStatement(string path) : base(Nodes.SourceFileStatement)
        {
            Path = path;
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override void Accept(AstVisitor visitor)
        {

        }
    }
}
