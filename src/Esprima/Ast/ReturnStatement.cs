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

        public override NodeCollection ChildNodes => new(Argument);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitReturnStatement(this);
        }

        public ReturnStatement UpdateWith(Expression? argument)
        {
            if (argument == Argument)
            {
                return this;
            }

            return new ReturnStatement(argument).SetAdditionalInfo(this);
        }
    }
}
