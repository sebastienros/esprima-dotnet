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

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitConditionalExpression(this) as T;
        }
    }
}
