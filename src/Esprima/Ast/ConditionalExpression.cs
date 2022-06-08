using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ConditionalExpression : Expression
    {
        public readonly Expression Test;
        public readonly Expression Consequent;
        public readonly Expression Alternate;

        public ConditionalExpression(
            Expression test,
            Expression consequent,
            Expression alternate) : base(Nodes.ConditionalExpression)
        {
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }

        public override NodeCollection ChildNodes => new(Test, Consequent, Alternate);

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitConditionalExpression(this);
        }

        public ConditionalExpression UpdateWith(Expression test, Expression consequent, Expression alternate)
        {
            if (test == Test && consequent == Consequent && alternate == Alternate)
            {
                return this;
            }

            return new ConditionalExpression(test, consequent, alternate).SetAdditionalInfo(this);
        }
    }
}
