using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class SpreadElement : Expression
    {
        public readonly Expression Argument;

        public SpreadElement(Expression argument) : base(Nodes.SpreadElement)
        {
            Argument = argument;
        }

        public override NodeCollection ChildNodes => new(Argument);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitSpreadElement(this);
        }

        public SpreadElement UpdateWith(Expression argument)
        {
            if (argument == Argument)
            {
                return this;
            }

            return new SpreadElement(argument).SetAdditionalInfo(this);
        }
    }
}
