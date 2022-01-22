using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class UndefStatement : Statement
    {
        public string Symbol { get; set; }

        public UndefStatement(string symbol) : base(Nodes.UndefStatement)
        {
            Symbol = symbol;
        }

        public override NodeCollection ChildNodes => new();

        protected internal override void Accept(AstVisitor visitor)
        {
            throw new System.Exception("Not implemented");
        }
    }
}
