using Esprima.Utils;

namespace Esprima.Ast
{
    public class FinalizerStatement : Statement
    {
        public Expression Body { get; }

        public FinalizerStatement(Expression body)
            : base(Nodes.FinalizerStatement)
        {
            Body = body;
        }

        public override NodeCollection ChildNodes => new();

        protected internal override void Accept(AstVisitor visitor)
        {

        }
    }

}
