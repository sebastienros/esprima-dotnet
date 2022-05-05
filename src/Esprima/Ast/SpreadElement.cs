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

        protected internal override T? Accept<T>(AstVisitor visitor) where T : class
        {
            return visitor.VisitSpreadElement(this) as T;
        }
    }
}
