namespace Esprima.Ast
{
    public class ArrowFunctionExpression : Node, Expression
    {
        public readonly Identifier Id;
        public readonly List<INode> Params;
        public readonly INode Body; // : BlockStatement | Expression;
        public readonly bool Generator;
        public readonly bool Expression;

        public HoistingScope HoistingScope;

        public ArrowFunctionExpression(List<INode> parameters, INode body, bool expression, HoistingScope hoistingScope) :
            base(Nodes.ArrowFunctionExpression)
        {
            Id = null;
            Params = parameters;
            Body = body;
            Generator = false;
            Expression = expression;
            HoistingScope = hoistingScope;
        }        
    }
}
