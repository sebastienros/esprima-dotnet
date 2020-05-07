namespace Esprima.Ast
{
    public sealed class SequenceExpression : Expression
    {
        private NodeList<Expression> _expressions;

        public SequenceExpression(in NodeList<Expression> expressions) : base(Nodes.SequenceExpression)
        {
            _expressions = expressions;
        }

        public ref readonly NodeList<Expression> Expressions => ref _expressions;

        internal void UpdateExpressions(in NodeList<Expression> value) => _expressions = value;

        public override NodeCollection ChildNodes => GenericChildNodeYield.Yield(Expressions);
    }
}