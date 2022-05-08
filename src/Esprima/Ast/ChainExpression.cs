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

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitChainExpression(this) as T;
        }
    }
}
