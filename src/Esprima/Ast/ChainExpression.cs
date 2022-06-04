using Esprima.Utils;

namespace Esprima.Ast
{
    public sealed class ChainExpression : Expression
    {
        /// <summary>
        /// CallExpression | ComputedMemberExpression | StaticMemberExpression
        /// </summary>
        public readonly Expression Expression;

        public ChainExpression(Expression expression) : base(Nodes.ChainExpression)
        {
            Expression = expression;
        }

        public override NodeCollection ChildNodes => NodeCollection.Empty;

        protected internal override object? Accept(AstVisitor visitor)
        {
            return visitor.VisitChainExpression(this);
        }

        public ChainExpression UpdateWith(Expression expression)
        {
            if (expression == Expression)
            {
                return this;
            }

            return new ChainExpression(expression);
        }
    }
}
