using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ReturnStatement : Statement
    {
        public readonly Expression? Argument;

        public ReturnStatement(Expression? argument) : base(Nodes.ReturnStatement)
        {
            Argument = argument;
        }

        public override NodeCollection ChildNodes => new NodeCollection(Argument);

        protected internal override void Accept(AstVisitor visitor) => visitor.VisitReturnStatement(this);
    }
}