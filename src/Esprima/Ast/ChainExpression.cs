using Esprima.Utils;

namespace Esprima.Ast
{
    public class ChainExpression : Expression
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

        public override void Accept(AstVisitor visitor) => visitor.VisitChainExpression(this);
    }
}